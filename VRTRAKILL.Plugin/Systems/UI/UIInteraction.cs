using System.Collections.Generic;
using Plugin.Systems.Controllers;
using Plugin.Systems.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR;

namespace Plugin.Systems.UI
{
    // controller laser pointer plus stick navigation; acts as the only pointer for converted canvases
    internal class UIInteraction : MonoBehaviour
    {
        public Camera Cam;

        // hand that owns the laser; the last trigger pressed takes it over
        public static SteamVR_Input_Sources ActiveHand = SteamVR_Input_Sources.RightHand;
        public static bool HasHit { get; private set; }
        public static Vector3 HitPoint { get; private set; }

        private const float DragAngle = 1.5f, NavPress = .6f, NavRelease = .35f, NavDelay = .4f, NavRepeat = .15f;

        private EventSystem ES;
        private PointerEventData Pointer;
        private readonly List<RaycastResult> Results = new List<RaycastResult>();

        private GameObject Hovered;
        private bool HoverMuted;
        private SteamVR_Input_Sources PressHand = SteamVR_Input_Sources.Any;
        private Vector3 PressForward;

        private bool NavHeld;
        private float NextNavTime;

        private static bool IsMenu => Vars.IsPaused;

        public void Start()
        {
            if (Cam == null) Cam = GetComponent<Camera>();
        }

        public void LateUpdate()
        {
            if (EventSystem.current == null || Cam == null) return;
            if (ES != EventSystem.current || Pointer == null) ResetPointer(EventSystem.current);
            HideModuleMouse();

            bool LeftDown = TriggerDown(SteamVR_Input_Sources.LeftHand), RightDown = TriggerDown(SteamVR_Input_Sources.RightHand);
            if (PressHand == SteamVR_Input_Sources.Any)
            {
                if (RightDown) ActiveHand = SteamVR_Input_Sources.RightHand;
                else if (LeftDown) ActiveHand = SteamVR_Input_Sources.LeftHand;
            }
            AttachToHand();

            GameObject Target = Raycast();
            UpdateHover(Target);

            if (PressHand == SteamVR_Input_Sources.Any && (ActiveHand == SteamVR_Input_Sources.RightHand ? RightDown : LeftDown))
                Press(Target);
            else if (PressHand != SteamVR_Input_Sources.Any)
            {
                if (TriggerHeld(PressHand)) Drag();
                else Release(Target);
            }

            if (IsMenu)
            {
                Navigate();
                if (SteamVR_Actions._default.ChangeWeaponVariation.GetStateDown(SteamVR_Input_Sources.Any)) Submit();
                Scroll();
            }
            else NavHeld = false;
        }

        #region Pointer
        private void ResetPointer(EventSystem NewES)
        {
            if (Pointer != null) { SetHover(null); ClearPress(); }
            ES = NewES;
            Pointer = new PointerEventData(ES) { button = PointerEventData.InputButton.Left, pointerId = -2 };
        }

        // the game's StandaloneInputModule also casts the OS mouse through this camera, which doubled every click
        private void HideModuleMouse()
        {
            if (!Vars.Config.UIInteraction.ControllerBased) return;
            BaseInputModule Module = ES.currentInputModule;
            if (Module == null || Module.inputOverride is NoMouseInput) return;
            Module.inputOverride = Module.GetComponent<NoMouseInput>() ?? Module.gameObject.AddComponent<NoMouseInput>();
        }

        private void AttachToHand()
        {
            if (!Vars.Config.UIInteraction.ControllerBased) return;
            VRControllersSystem Hand = VRControllersSystem.Get(ActiveHand);
            if (Hand == null || Hand.GunOffset == null || transform.parent == Hand.GunOffset.transform) return;
            transform.SetParent(Hand.GunOffset.transform, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        private GameObject Raycast()
        {
            Pointer.position = new Vector2(Cam.pixelWidth / 2f, Cam.pixelHeight / 2f);
            Pointer.delta = Vector2.zero;
            Results.Clear();
            ES.RaycastAll(Pointer, Results);

            RaycastResult First = default;
            foreach (RaycastResult R in Results)
                if (R.gameObject != null) { First = R; break; }
            Results.Clear();

            Pointer.pointerCurrentRaycast = First;
            HasHit = First.gameObject != null;
            if (HasHit) HitPoint = First.worldPosition;
            return First.gameObject;
        }

        private void UpdateHover(GameObject Target)
        {
            if (Target == Hovered) return;
            HoverMuted = false;
            SetHover(Target);

            // pointing at an item also selects it, so the stick and confirm continue from there
            if (IsMenu && Target != null && Target.GetComponentInParent<Selectable>() is Selectable S && CanSelect(S) && !IsTextField(S))
                Select(S);
        }

        // sends exit/enter to every object between the old and new target, like PointerInputModule
        private void SetHover(GameObject Target)
        {
            Transform Root = CommonRoot(Hovered, Target);
            if (Hovered != null && !HoverMuted)
                for (Transform T = Hovered.transform; T != null && T != Root; T = T.parent)
                    ExecuteEvents.Execute(T.gameObject, Pointer, ExecuteEvents.pointerExitHandler);
            Pointer.hovered.Clear();
            if (Target != null)
                for (Transform T = Target.transform; T != null && T != Root; T = T.parent)
                {
                    ExecuteEvents.Execute(T.gameObject, Pointer, ExecuteEvents.pointerEnterHandler);
                    Pointer.hovered.Add(T.gameObject);
                }
            Hovered = Target;
            Pointer.pointerEnter = Target;
        }

        // stops the laser highlight once the stick takes over, until the laser moves to another item
        private void MuteHover()
        {
            if (Hovered == null || HoverMuted) return;
            for (Transform T = Hovered.transform; T != null; T = T.parent)
                ExecuteEvents.Execute(T.gameObject, Pointer, ExecuteEvents.pointerExitHandler);
            HoverMuted = true;
        }

        private static Transform CommonRoot(GameObject A, GameObject B)
        {
            if (A == null || B == null) return null;
            for (Transform T = A.transform; T != null; T = T.parent)
                if (B.transform.IsChildOf(T)) return T;
            return null;
        }

        private void Press(GameObject Target)
        {
            // the game's own ControllerPointer handles its screens (shop, terminals) through Fire1
            if (Target == null || Target.GetComponentInParent<ControllerPointer>() != null) return;

            PressHand = ActiveHand;
            PressForward = transform.forward;
            Pointer.eligibleForClick = true;
            Pointer.dragging = false;
            Pointer.useDragThreshold = true;
            Pointer.pressPosition = Pointer.position;
            Pointer.pointerPressRaycast = Pointer.pointerCurrentRaycast;

            GameObject Pressed = ExecuteEvents.ExecuteHierarchy(Target, Pointer, ExecuteEvents.pointerDownHandler)
                              ?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(Target);
            Pointer.pointerPress = Pressed;
            Pointer.rawPointerPress = Target;
            Pointer.clickTime = Time.unscaledTime;
            Pointer.clickCount = 1;

            Pointer.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(Target);
            if (Pointer.pointerDrag != null)
                ExecuteEvents.Execute(Pointer.pointerDrag, Pointer, ExecuteEvents.initializePotentialDrag);
        }

        private void Drag()
        {
            if (Pointer.pointerDrag == null) return;
            // the laser always casts from the screen center, so movement is the hand's angle since the press
            if (!Pointer.dragging && (!Pointer.useDragThreshold || Vector3.Angle(PressForward, transform.forward) > DragAngle))
            {
                ExecuteEvents.Execute(Pointer.pointerDrag, Pointer, ExecuteEvents.beginDragHandler);
                Pointer.dragging = true;
                Pointer.eligibleForClick = Pointer.pointerDrag == Pointer.pointerPress && !Pointer.useDragThreshold;
            }
            if (Pointer.dragging) ExecuteEvents.Execute(Pointer.pointerDrag, Pointer, ExecuteEvents.dragHandler);
        }

        private void Release(GameObject Target)
        {
            if (Pointer.pointerPress != null)
                ExecuteEvents.Execute(Pointer.pointerPress, Pointer, ExecuteEvents.pointerUpHandler);

            GameObject ClickHandler = Target != null ? ExecuteEvents.GetEventHandler<IPointerClickHandler>(Target) : null;
            if (Pointer.pointerPress != null && Pointer.pointerPress == ClickHandler && Pointer.eligibleForClick)
                ExecuteEvents.Execute(Pointer.pointerPress, Pointer, ExecuteEvents.pointerClickHandler);
            else if (Pointer.dragging && Target != null)
                ExecuteEvents.ExecuteHierarchy(Target, Pointer, ExecuteEvents.dropHandler);

            if (Pointer.dragging && Pointer.pointerDrag != null)
                ExecuteEvents.Execute(Pointer.pointerDrag, Pointer, ExecuteEvents.endDragHandler);
            ClearPress();
        }

        private void ClearPress()
        {
            PressHand = SteamVR_Input_Sources.Any;
            Pointer.eligibleForClick = false;
            Pointer.dragging = false;
            Pointer.pointerPress = null;
            Pointer.rawPointerPress = null;
            Pointer.pointerDrag = null;
        }

        private static bool TriggerDown(SteamVR_Input_Sources Hand)
            => SteamVR_Actions._default.Shoot.GetStateDown(Hand) || SteamVR_Actions._default.Punch.GetStateDown(Hand);
        private static bool TriggerHeld(SteamVR_Input_Sources Hand)
            => SteamVR_Actions._default.Shoot.GetState(Hand) || SteamVR_Actions._default.Punch.GetState(Hand);
        #endregion

        #region Stick navigation
        private void Navigate()
        {
            Vector2 Axis = InputVars.MoveVector;
            float Max = Mathf.Max(Mathf.Abs(Axis.x), Mathf.Abs(Axis.y));
            if (Max < NavRelease) { NavHeld = false; return; }
            if (Max < NavPress) return;
            if (NavHeld && Time.unscaledTime < NextNavTime) return;
            NextNavTime = Time.unscaledTime + (NavHeld ? NavRepeat : NavDelay);
            NavHeld = true;

            MuteHover();
            Selectable Current = CurrentSelection();
            if (Current == null) { Select(InitialSelection()); return; }

            // same move event the game's input module sends for a gamepad, so sliders and custom handlers react too
            AxisEventData Move = new AxisEventData(ES)
            {
                moveVector = Axis,
                moveDir = Mathf.Abs(Axis.x) > Mathf.Abs(Axis.y)
                    ? (Axis.x > 0 ? MoveDirection.Right : MoveDirection.Left)
                    : (Axis.y > 0 ? MoveDirection.Up : MoveDirection.Down)
            };
            ExecuteEvents.Execute(Current.gameObject, Move, ExecuteEvents.moveHandler);
            if (CurrentSelection() is Selectable Next) Tint(Next);
        }

        private void Submit()
        {
            Selectable Current = CurrentSelection();
            if (Current == null) { MuteHover(); Select(InitialSelection()); return; }
            ExecuteEvents.Execute(Current.gameObject, new BaseEventData(ES), ExecuteEvents.submitHandler);
        }

        // ScrollRects with the game's ScrollRectMouseControl already follow the simulated wheel
        private void Scroll()
        {
            float Y = InputVars.TurnVector.y;
            if (Hovered == null || Mathf.Abs(Y) < Vars.Config.Controllers.Deadzone * 1.5f) return;
            GameObject Handler = ExecuteEvents.GetEventHandler<IScrollHandler>(Hovered);
            if (Handler == null || Handler.GetComponent<ScrollRectMouseControl>() != null) return;
            Pointer.scrollDelta = new Vector2(0, Mathf.Sign(Y));
            ExecuteEvents.Execute(Handler, Pointer, ExecuteEvents.scrollHandler);
            Pointer.scrollDelta = Vector2.zero;
        }

        private Selectable CurrentSelection()
        {
            GameObject GO = ES.currentSelectedGameObject;
            Selectable S = GO != null ? GO.GetComponentInParent<Selectable>() : null;
            return CanSelect(S) ? S : null;
        }

        // the game only picks a default item for gamepads, so pick its GamepadObjectSelector target ourselves
        private Selectable InitialSelection()
        {
            if (Hovered != null && Hovered.GetComponentInParent<Selectable>() is Selectable Pointed && CanSelect(Pointed)) return Pointed;

            if (TopSelector() is GamepadObjectSelector Top && Top != null && Top.isActiveAndEnabled && Top.target != null)
            {
                if (Top.firstChild)
                    foreach (Selectable Child in Top.target.GetComponentsInChildren<Selectable>())
                        if (Child.gameObject != Top.target && Child.navigation.mode != Navigation.Mode.None && CanSelect(Child)) return Child;
                if (Top.target.GetComponent<Selectable>() is Selectable S && CanSelect(S)) return S;
            }

            Selectable Best = null; int BestOrder = int.MinValue;
            foreach (Selectable S in Selectable.allSelectablesArray)
            {
                if (!CanSelect(S) || S.navigation.mode == Navigation.Mode.None) continue;
                Canvas C = S.GetComponentInParent<Canvas>();
                int Order = C != null && C.rootCanvas != null ? C.rootCanvas.sortingOrder : 0;
                if (Best == null || Order > BestOrder) { Best = S; BestOrder = Order; }
            }
            return Best;
        }

        // s_Selectors is a Stack<> the plugin's reference set cannot resolve, so read it untyped; a stack enumerates top first
        private static readonly System.Reflection.FieldInfo SelectorsField = typeof(GamepadObjectSelector).GetField("s_Selectors",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        private static GamepadObjectSelector TopSelector()
        {
            if (SelectorsField?.GetValue(null) is System.Collections.IEnumerable Stack)
                foreach (object Top in Stack) return Top as GamepadObjectSelector;
            return null;
        }

        private void Select(Selectable S)
        {
            if (S == null || ES.currentSelectedGameObject == S.gameObject) return;
            Tint(S);
            ES.SetSelectedGameObject(S.gameObject, new BaseEventData(ES));
        }

        // converted canvases only recolor the hover state, so the stick selection would be invisible
        private static void Tint(Selectable S)
        {
            if (S.transition != Selectable.Transition.ColorTint || S.colors.selectedColor == S.colors.highlightedColor) return;
            ColorBlock Block = S.colors;
            Block.selectedColor = Block.highlightedColor;
            S.colors = Block;
        }

        private static bool CanSelect(Selectable S)
            => S != null && S.isActiveAndEnabled && S.gameObject.activeInHierarchy && S.IsInteractable();

        // selecting a text field opens it for typing, so the laser only highlights those
        private static bool IsTextField(Selectable S)
            => S is InputField || S.GetType().Name == "TMP_InputField";
        #endregion
    }

    // hides the OS mouse from the game's input module; keyboard submit/cancel still go through
    internal sealed class NoMouseInput : BaseInput
    {
        public override bool mousePresent => false;
        public override bool GetMouseButtonDown(int button) => false;
        public override bool GetMouseButtonUp(int button) => false;
        public override bool GetMouseButton(int button) => false;
        public override Vector2 mouseScrollDelta => Vector2.zero;
    }
}
