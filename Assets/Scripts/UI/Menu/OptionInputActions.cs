// GENERATED AUTOMATICALLY FROM 'Assets/InputActions/Menu/OptionInputActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @OptionInputActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @OptionInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""OptionInputActions"",
    ""maps"": [
        {
            ""name"": ""Option"",
            ""id"": ""0632aa96-8875-4c33-8c65-8588f3e329bc"",
            ""actions"": [
                {
                    ""name"": ""Click"",
                    ""type"": ""Button"",
                    ""id"": ""a779c315-b26c-4ca8-841e-837063c7d4e5"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""427f3935-4a7b-4fae-a6e3-aa04587368ca"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Vertical"",
                    ""type"": ""Value"",
                    ""id"": ""dd3861be-f737-4a29-8d36-67326db2fe53"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Close"",
                    ""type"": ""Button"",
                    ""id"": ""3ecafdf8-4542-4fc0-860e-fa22b55ed976"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""a2b8012e-4cec-49ef-947a-e7bdba53392a"",
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
                    ""id"": ""63748e21-ad7b-472d-b1e1-5475c03f7047"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f382cc23-ee22-4125-b4bf-ce522670a00c"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""dc5418ae-f7c1-40ae-b074-1bd5c5c019cc"",
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
                    ""id"": ""6b8cc56d-966b-4160-94f0-21ee5b39118d"",
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
                    ""id"": ""8f388331-f584-43ba-aead-8a9618e33183"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""2D Vector Controller"",
                    ""id"": ""5ef40b8d-e9e6-4763-a6ef-6c50c1f69f0c"",
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
                    ""id"": ""94124f7d-0ba8-4f7a-9ad0-651feb5917d3"",
                    ""path"": ""<Gamepad>/dpad/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""36501ecd-ff1e-409f-92b7-2d6064a15385"",
                    ""path"": ""<Gamepad>/dpad/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""a2cf1b9e-ce41-4dc9-a05e-07c026bc2e8e"",
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
                    ""id"": ""de057e6e-da2b-4c97-bfa7-b9f6acce2998"",
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
                    ""id"": ""112fd07d-3ea3-4452-95fb-7664f75abd04"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Vertical"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""2D Vector Controller"",
                    ""id"": ""84052998-3fc6-46c0-bd2b-b25f82a65a9e"",
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
                    ""id"": ""7f0a441e-eae8-4cae-aeb8-e318e9c869a2"",
                    ""path"": ""<Gamepad>/dpad/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Vertical"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""106e4f57-eb66-4132-a101-04943d72d622"",
                    ""path"": ""<Gamepad>/dpad/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Vertical"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""bb449544-b026-4a9b-956d-87375a6de14c"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Close"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d7dddfd0-131c-4fc2-bfb6-46d9c9ff85f5"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Close"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Option
        m_Option = asset.FindActionMap("Option", throwIfNotFound: true);
        m_Option_Click = m_Option.FindAction("Click", throwIfNotFound: true);
        m_Option_Move = m_Option.FindAction("Move", throwIfNotFound: true);
        m_Option_Vertical = m_Option.FindAction("Vertical", throwIfNotFound: true);
        m_Option_Close = m_Option.FindAction("Close", throwIfNotFound: true);
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

    // Option
    private readonly InputActionMap m_Option;
    private IOptionActions m_OptionActionsCallbackInterface;
    private readonly InputAction m_Option_Click;
    private readonly InputAction m_Option_Move;
    private readonly InputAction m_Option_Vertical;
    private readonly InputAction m_Option_Close;
    public struct OptionActions
    {
        private @OptionInputActions m_Wrapper;
        public OptionActions(@OptionInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Click => m_Wrapper.m_Option_Click;
        public InputAction @Move => m_Wrapper.m_Option_Move;
        public InputAction @Vertical => m_Wrapper.m_Option_Vertical;
        public InputAction @Close => m_Wrapper.m_Option_Close;
        public InputActionMap Get() { return m_Wrapper.m_Option; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(OptionActions set) { return set.Get(); }
        public void SetCallbacks(IOptionActions instance)
        {
            if (m_Wrapper.m_OptionActionsCallbackInterface != null)
            {
                @Click.started -= m_Wrapper.m_OptionActionsCallbackInterface.OnClick;
                @Click.performed -= m_Wrapper.m_OptionActionsCallbackInterface.OnClick;
                @Click.canceled -= m_Wrapper.m_OptionActionsCallbackInterface.OnClick;
                @Move.started -= m_Wrapper.m_OptionActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_OptionActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_OptionActionsCallbackInterface.OnMove;
                @Vertical.started -= m_Wrapper.m_OptionActionsCallbackInterface.OnVertical;
                @Vertical.performed -= m_Wrapper.m_OptionActionsCallbackInterface.OnVertical;
                @Vertical.canceled -= m_Wrapper.m_OptionActionsCallbackInterface.OnVertical;
                @Close.started -= m_Wrapper.m_OptionActionsCallbackInterface.OnClose;
                @Close.performed -= m_Wrapper.m_OptionActionsCallbackInterface.OnClose;
                @Close.canceled -= m_Wrapper.m_OptionActionsCallbackInterface.OnClose;
            }
            m_Wrapper.m_OptionActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Click.started += instance.OnClick;
                @Click.performed += instance.OnClick;
                @Click.canceled += instance.OnClick;
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Vertical.started += instance.OnVertical;
                @Vertical.performed += instance.OnVertical;
                @Vertical.canceled += instance.OnVertical;
                @Close.started += instance.OnClose;
                @Close.performed += instance.OnClose;
                @Close.canceled += instance.OnClose;
            }
        }
    }
    public OptionActions @Option => new OptionActions(this);
    public interface IOptionActions
    {
        void OnClick(InputAction.CallbackContext context);
        void OnMove(InputAction.CallbackContext context);
        void OnVertical(InputAction.CallbackContext context);
        void OnClose(InputAction.CallbackContext context);
    }
}
