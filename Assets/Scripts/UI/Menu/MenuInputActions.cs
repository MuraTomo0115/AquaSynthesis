// GENERATED AUTOMATICALLY FROM 'Assets/InputActions/Menu/MenuInputActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @MenuInputActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @MenuInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""MenuInputActions"",
    ""maps"": [
        {
            ""name"": ""Menu"",
            ""id"": ""8bd442ab-31d4-43e3-a4fb-9d5b55b6c5d3"",
            ""actions"": [
                {
                    ""name"": ""Open"",
                    ""type"": ""Button"",
                    ""id"": ""89f4a9af-a9a0-4236-9f38-5092ed6e82d3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""c2dbdc0c-2d4f-4ecb-8545-3df00e1d24e8"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Click"",
                    ""type"": ""Button"",
                    ""id"": ""a3ede569-db8e-412f-8ce6-629892580be2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Scroll"",
                    ""type"": ""PassThrough"",
                    ""id"": ""d229c502-30d2-4950-a406-ff344143457a"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""New action"",
                    ""type"": ""Button"",
                    ""id"": ""421db2da-7e2c-4128-9072-cb56e39434fd"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Vertical"",
                    ""type"": ""Value"",
                    ""id"": ""c92f3ec0-bd12-4a45-b773-699b666ba9bb"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""83f5dc04-0eb3-4e85-916a-0639eb5321c8"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Open"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c15ca90b-b509-4c52-a6c1-75d066c1f5b4"",
                    ""path"": ""<Gamepad>/start"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Open"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f3242fda-cbf6-4f6b-ba4d-bd82aeac0448"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""dd30c9f8-9e09-4876-bac4-477ac8752b10"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2c933e34-b32c-4737-b293-a5255f628b3c"",
                    ""path"": ""<Mouse>/scroll"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Scroll"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6f77904f-c3bf-4916-8739-92964c365e3d"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""New action"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""b8805f63-0935-4a74-afe9-ee8eb755e67b"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""left"",
                    ""id"": ""4af13d4c-3ca3-4d2f-9c9a-288d9676d1af"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""67316884-f12e-44a0-950a-2918d1c848fe"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""13b38a5e-8230-41e0-b66b-ca0de7bf5acf"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""7c7532f5-06d9-4229-9206-409043c42885"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Vertical"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""f807d6d0-084f-411a-96cb-e9f48a72b2f4"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Vertical"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""c216049a-5cbe-44b5-9c4a-55bc077b23bc"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Vertical"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Menu
        m_Menu = asset.FindActionMap("Menu", throwIfNotFound: true);
        m_Menu_Open = m_Menu.FindAction("Open", throwIfNotFound: true);
        m_Menu_Move = m_Menu.FindAction("Move", throwIfNotFound: true);
        m_Menu_Click = m_Menu.FindAction("Click", throwIfNotFound: true);
        m_Menu_Scroll = m_Menu.FindAction("Scroll", throwIfNotFound: true);
        m_Menu_Newaction = m_Menu.FindAction("New action", throwIfNotFound: true);
        m_Menu_Vertical = m_Menu.FindAction("Vertical", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Menu
    private readonly InputActionMap m_Menu;
    private IMenuActions m_MenuActionsCallbackInterface;
    private readonly InputAction m_Menu_Open;
    private readonly InputAction m_Menu_Move;
    private readonly InputAction m_Menu_Click;
    private readonly InputAction m_Menu_Scroll;
    private readonly InputAction m_Menu_Newaction;
    private readonly InputAction m_Menu_Vertical;
    public struct MenuActions
    {
        private @MenuInputActions m_Wrapper;
        public MenuActions(@MenuInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Open => m_Wrapper.m_Menu_Open;
        public InputAction @Move => m_Wrapper.m_Menu_Move;
        public InputAction @Click => m_Wrapper.m_Menu_Click;
        public InputAction @Scroll => m_Wrapper.m_Menu_Scroll;
        public InputAction @Newaction => m_Wrapper.m_Menu_Newaction;
        public InputAction @Vertical => m_Wrapper.m_Menu_Vertical;
        public InputActionMap Get() { return m_Wrapper.m_Menu; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MenuActions set) { return set.Get(); }
        public void SetCallbacks(IMenuActions instance)
        {
            if (m_Wrapper.m_MenuActionsCallbackInterface != null)
            {
                @Open.started -= m_Wrapper.m_MenuActionsCallbackInterface.OnOpen;
                @Open.performed -= m_Wrapper.m_MenuActionsCallbackInterface.OnOpen;
                @Open.canceled -= m_Wrapper.m_MenuActionsCallbackInterface.OnOpen;
                @Move.started -= m_Wrapper.m_MenuActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_MenuActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_MenuActionsCallbackInterface.OnMove;
                @Click.started -= m_Wrapper.m_MenuActionsCallbackInterface.OnClick;
                @Click.performed -= m_Wrapper.m_MenuActionsCallbackInterface.OnClick;
                @Click.canceled -= m_Wrapper.m_MenuActionsCallbackInterface.OnClick;
                @Scroll.started -= m_Wrapper.m_MenuActionsCallbackInterface.OnScroll;
                @Scroll.performed -= m_Wrapper.m_MenuActionsCallbackInterface.OnScroll;
                @Scroll.canceled -= m_Wrapper.m_MenuActionsCallbackInterface.OnScroll;
                @Newaction.started -= m_Wrapper.m_MenuActionsCallbackInterface.OnNewaction;
                @Newaction.performed -= m_Wrapper.m_MenuActionsCallbackInterface.OnNewaction;
                @Newaction.canceled -= m_Wrapper.m_MenuActionsCallbackInterface.OnNewaction;
                @Vertical.started -= m_Wrapper.m_MenuActionsCallbackInterface.OnVertical;
                @Vertical.performed -= m_Wrapper.m_MenuActionsCallbackInterface.OnVertical;
                @Vertical.canceled -= m_Wrapper.m_MenuActionsCallbackInterface.OnVertical;
            }
            m_Wrapper.m_MenuActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Open.started += instance.OnOpen;
                @Open.performed += instance.OnOpen;
                @Open.canceled += instance.OnOpen;
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Click.started += instance.OnClick;
                @Click.performed += instance.OnClick;
                @Click.canceled += instance.OnClick;
                @Scroll.started += instance.OnScroll;
                @Scroll.performed += instance.OnScroll;
                @Scroll.canceled += instance.OnScroll;
                @Newaction.started += instance.OnNewaction;
                @Newaction.performed += instance.OnNewaction;
                @Newaction.canceled += instance.OnNewaction;
                @Vertical.started += instance.OnVertical;
                @Vertical.performed += instance.OnVertical;
                @Vertical.canceled += instance.OnVertical;
            }
        }
    }
    public MenuActions @Menu => new MenuActions(this);
    public interface IMenuActions
    {
        void OnOpen(InputAction.CallbackContext context);
        void OnMove(InputAction.CallbackContext context);
        void OnClick(InputAction.CallbackContext context);
        void OnScroll(InputAction.CallbackContext context);
        void OnNewaction(InputAction.CallbackContext context);
        void OnVertical(InputAction.CallbackContext context);
    }
}
