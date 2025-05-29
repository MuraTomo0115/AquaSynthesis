// GENERATED AUTOMATICALLY FROM 'Assets/InputActions/StageSelect/StageSelectInputActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @StageSelectInputActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @StageSelectInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""StageSelectInputActions"",
    ""maps"": [
        {
            ""name"": ""StageSelect"",
            ""id"": ""d12f6604-6894-4824-a3fe-16cdbf000010"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""f7b158e7-8e3a-4bd5-8f77-132212683c58"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Submit"",
                    ""type"": ""Button"",
                    ""id"": ""4dedcc09-4c83-4346-85e2-d2d7bedc9944"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""Keyboard"",
                    ""id"": ""e25b61f2-a714-40e2-afc0-8a5601320528"",
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
                    ""id"": ""3373f7f3-f1d2-4e0a-bfcf-495b1753b030"",
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
                    ""id"": ""ab935940-7fb9-4427-9e31-ec680a0dd273"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Gamepad"",
                    ""id"": ""af60a4ff-1c61-4d3e-b244-a68f1ef03efb"",
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
                    ""id"": ""7edc92d1-6104-4b0f-a3ca-2c3478c60d7a"",
                    ""path"": ""<Gamepad>/dpad/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""ff291a7f-8d32-42e2-af5b-85b916bda55f"",
                    ""path"": ""<Gamepad>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""02159711-e619-4103-bf65-bd08d379f3f6"",
                    ""path"": ""<Gamepad>/dpad/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""9a6c9fef-2f89-4ede-95d7-da6e9e25d822"",
                    ""path"": ""<Gamepad>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""85d3abf0-2e44-4427-8db5-f57f48d766a7"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Submit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""42ba2de9-84bc-484d-8a6d-9e7f8b8ac8db"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Submit"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // StageSelect
        m_StageSelect = asset.FindActionMap("StageSelect", throwIfNotFound: true);
        m_StageSelect_Move = m_StageSelect.FindAction("Move", throwIfNotFound: true);
        m_StageSelect_Submit = m_StageSelect.FindAction("Submit", throwIfNotFound: true);
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

    // StageSelect
    private readonly InputActionMap m_StageSelect;
    private IStageSelectActions m_StageSelectActionsCallbackInterface;
    private readonly InputAction m_StageSelect_Move;
    private readonly InputAction m_StageSelect_Submit;
    public struct StageSelectActions
    {
        private @StageSelectInputActions m_Wrapper;
        public StageSelectActions(@StageSelectInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_StageSelect_Move;
        public InputAction @Submit => m_Wrapper.m_StageSelect_Submit;
        public InputActionMap Get() { return m_Wrapper.m_StageSelect; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(StageSelectActions set) { return set.Get(); }
        public void SetCallbacks(IStageSelectActions instance)
        {
            if (m_Wrapper.m_StageSelectActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_StageSelectActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_StageSelectActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_StageSelectActionsCallbackInterface.OnMove;
                @Submit.started -= m_Wrapper.m_StageSelectActionsCallbackInterface.OnSubmit;
                @Submit.performed -= m_Wrapper.m_StageSelectActionsCallbackInterface.OnSubmit;
                @Submit.canceled -= m_Wrapper.m_StageSelectActionsCallbackInterface.OnSubmit;
            }
            m_Wrapper.m_StageSelectActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Submit.started += instance.OnSubmit;
                @Submit.performed += instance.OnSubmit;
                @Submit.canceled += instance.OnSubmit;
            }
        }
    }
    public StageSelectActions @StageSelect => new StageSelectActions(this);
    public interface IStageSelectActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnSubmit(InputAction.CallbackContext context);
    }
}
