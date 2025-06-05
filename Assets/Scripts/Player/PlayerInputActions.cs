// GENERATED AUTOMATICALLY FROM 'Assets/InputActions/Player/PlayerInputActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerInputActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInputActions"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""d719b55e-f298-4203-837d-3e34a4536ec6"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""cdb69c37-f8cd-4248-9379-621ab3a1c076"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""3ab28062-5597-445f-b8ca-5638875a3fe6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Attack"",
                    ""type"": ""Button"",
                    ""id"": ""2ba7bb24-9e72-4be1-a1bc-dfe8876096f1"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Pistol"",
                    ""type"": ""Button"",
                    ""id"": ""155aeb09-ed91-408e-8c34-103e1902a81c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Action"",
                    ""type"": ""Button"",
                    ""id"": ""d184d123-64f7-44e9-9ccc-034a37c48ac9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""AbilitySave"",
                    ""type"": ""Button"",
                    ""id"": ""6b1f6897-c9b9-4571-ae95-5ca2e31023bf"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""AbilityPlay"",
                    ""type"": ""Button"",
                    ""id"": ""bbdddb36-c880-4028-bc67-3d6f23975009"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""14eba34e-1177-4c1f-a941-301ca2003de2"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""ac225e7f-11d7-43dc-bac7-18af93f97e15"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""ef0c4dd2-e836-4774-9184-f13f6d9d6492"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""017d27ce-36d4-409b-bcf4-1b479ac1fb40"",
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
                    ""id"": ""08ec25b2-4cdb-45e9-adad-0482bfd11353"",
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
                    ""id"": ""67f3ae3f-df1a-497e-85e0-e61356df4d09"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6b23f16c-566f-4452-84e8-37f97db0e340"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""66848638-1b57-4c5e-8deb-6a9c62cae0fa"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""27a18e57-5c62-41f7-994b-a7c0f51c6e62"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Attack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4ddbe945-b464-41f4-b06c-beff1eb4221d"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Attack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1b678adb-84dd-4f23-8632-64242b4b767a"",
                    ""path"": ""<Keyboard>/p"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pistol"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""caa5a23a-2e71-456b-8911-fa31776b58e5"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pistol"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""fa930056-2667-474b-b432-32a4626c8e18"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Action"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bda97030-0e68-486f-97bb-807a117e8ca8"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Action"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d92dc917-cda9-42df-bebb-6d4bebbe882d"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""AbilitySave"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""092fd7da-c0fb-494c-adef-953cb24a272e"",
                    ""path"": ""<Gamepad>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""AbilitySave"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bc294706-818b-46fd-b78b-66440ceb4856"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""AbilityPlay"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2328d74b-67f3-45cd-b2c1-dcd14f1324e8"",
                    ""path"": ""<Gamepad>/leftShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""AbilityPlay"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""ADV"",
            ""id"": ""82587ca8-cfea-4183-8f36-902a45ba016a"",
            ""actions"": [
                {
                    ""name"": ""Advance"",
                    ""type"": ""Button"",
                    ""id"": ""e8f8ec8a-89c2-4cfd-ae18-66e992099b79"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""StartDemo"",
                    ""type"": ""Button"",
                    ""id"": ""96b1355e-315b-4018-b1a3-3a94c758d107"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""4f98b24e-7edb-4130-b9a9-88a4534135d8"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Advance"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""974eef50-7613-4a10-99b6-53c23b36ce95"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Advance"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a7bdfed7-7bde-4b63-b74c-84eda7d95516"",
                    ""path"": ""<Keyboard>/u"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""StartDemo"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Support"",
            ""id"": ""6797e531-711e-4d82-8dd9-beef860c26c2"",
            ""actions"": [
                {
                    ""name"": ""SummonA"",
                    ""type"": ""Button"",
                    ""id"": ""87c433e1-40d7-474a-b9d7-d00e3a570419"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SummonB"",
                    ""type"": ""Button"",
                    ""id"": ""96472615-a91b-47e0-8fd0-d96e8eb3eab3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""a924b5ab-6d58-43d2-a7f6-54066406e904"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SummonA"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""58d36789-9ce8-4464-b906-76e45dcd0614"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SummonA"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1c1bca38-1881-4927-8413-785a1ecf9bb2"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SummonB"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4bccea87-92c6-45bd-ab37-a8ee0127d971"",
                    ""path"": ""<Gamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SummonB"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Player
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_Move = m_Player.FindAction("Move", throwIfNotFound: true);
        m_Player_Jump = m_Player.FindAction("Jump", throwIfNotFound: true);
        m_Player_Attack = m_Player.FindAction("Attack", throwIfNotFound: true);
        m_Player_Pistol = m_Player.FindAction("Pistol", throwIfNotFound: true);
        m_Player_Action = m_Player.FindAction("Action", throwIfNotFound: true);
        m_Player_AbilitySave = m_Player.FindAction("AbilitySave", throwIfNotFound: true);
        m_Player_AbilityPlay = m_Player.FindAction("AbilityPlay", throwIfNotFound: true);
        // ADV
        m_ADV = asset.FindActionMap("ADV", throwIfNotFound: true);
        m_ADV_Advance = m_ADV.FindAction("Advance", throwIfNotFound: true);
        m_ADV_StartDemo = m_ADV.FindAction("StartDemo", throwIfNotFound: true);
        // Support
        m_Support = asset.FindActionMap("Support", throwIfNotFound: true);
        m_Support_SummonA = m_Support.FindAction("SummonA", throwIfNotFound: true);
        m_Support_SummonB = m_Support.FindAction("SummonB", throwIfNotFound: true);
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

    // Player
    private readonly InputActionMap m_Player;
    private IPlayerActions m_PlayerActionsCallbackInterface;
    private readonly InputAction m_Player_Move;
    private readonly InputAction m_Player_Jump;
    private readonly InputAction m_Player_Attack;
    private readonly InputAction m_Player_Pistol;
    private readonly InputAction m_Player_Action;
    private readonly InputAction m_Player_AbilitySave;
    private readonly InputAction m_Player_AbilityPlay;
    public struct PlayerActions
    {
        private @PlayerInputActions m_Wrapper;
        public PlayerActions(@PlayerInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_Player_Move;
        public InputAction @Jump => m_Wrapper.m_Player_Jump;
        public InputAction @Attack => m_Wrapper.m_Player_Attack;
        public InputAction @Pistol => m_Wrapper.m_Player_Pistol;
        public InputAction @Action => m_Wrapper.m_Player_Action;
        public InputAction @AbilitySave => m_Wrapper.m_Player_AbilitySave;
        public InputAction @AbilityPlay => m_Wrapper.m_Player_AbilityPlay;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Jump.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @Attack.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttack;
                @Attack.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttack;
                @Attack.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAttack;
                @Pistol.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPistol;
                @Pistol.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPistol;
                @Pistol.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPistol;
                @Action.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAction;
                @Action.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAction;
                @Action.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAction;
                @AbilitySave.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAbilitySave;
                @AbilitySave.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAbilitySave;
                @AbilitySave.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAbilitySave;
                @AbilityPlay.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAbilityPlay;
                @AbilityPlay.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAbilityPlay;
                @AbilityPlay.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAbilityPlay;
            }
            m_Wrapper.m_PlayerActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
                @Attack.started += instance.OnAttack;
                @Attack.performed += instance.OnAttack;
                @Attack.canceled += instance.OnAttack;
                @Pistol.started += instance.OnPistol;
                @Pistol.performed += instance.OnPistol;
                @Pistol.canceled += instance.OnPistol;
                @Action.started += instance.OnAction;
                @Action.performed += instance.OnAction;
                @Action.canceled += instance.OnAction;
                @AbilitySave.started += instance.OnAbilitySave;
                @AbilitySave.performed += instance.OnAbilitySave;
                @AbilitySave.canceled += instance.OnAbilitySave;
                @AbilityPlay.started += instance.OnAbilityPlay;
                @AbilityPlay.performed += instance.OnAbilityPlay;
                @AbilityPlay.canceled += instance.OnAbilityPlay;
            }
        }
    }
    public PlayerActions @Player => new PlayerActions(this);

    // ADV
    private readonly InputActionMap m_ADV;
    private IADVActions m_ADVActionsCallbackInterface;
    private readonly InputAction m_ADV_Advance;
    private readonly InputAction m_ADV_StartDemo;
    public struct ADVActions
    {
        private @PlayerInputActions m_Wrapper;
        public ADVActions(@PlayerInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Advance => m_Wrapper.m_ADV_Advance;
        public InputAction @StartDemo => m_Wrapper.m_ADV_StartDemo;
        public InputActionMap Get() { return m_Wrapper.m_ADV; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(ADVActions set) { return set.Get(); }
        public void SetCallbacks(IADVActions instance)
        {
            if (m_Wrapper.m_ADVActionsCallbackInterface != null)
            {
                @Advance.started -= m_Wrapper.m_ADVActionsCallbackInterface.OnAdvance;
                @Advance.performed -= m_Wrapper.m_ADVActionsCallbackInterface.OnAdvance;
                @Advance.canceled -= m_Wrapper.m_ADVActionsCallbackInterface.OnAdvance;
                @StartDemo.started -= m_Wrapper.m_ADVActionsCallbackInterface.OnStartDemo;
                @StartDemo.performed -= m_Wrapper.m_ADVActionsCallbackInterface.OnStartDemo;
                @StartDemo.canceled -= m_Wrapper.m_ADVActionsCallbackInterface.OnStartDemo;
            }
            m_Wrapper.m_ADVActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Advance.started += instance.OnAdvance;
                @Advance.performed += instance.OnAdvance;
                @Advance.canceled += instance.OnAdvance;
                @StartDemo.started += instance.OnStartDemo;
                @StartDemo.performed += instance.OnStartDemo;
                @StartDemo.canceled += instance.OnStartDemo;
            }
        }
    }
    public ADVActions @ADV => new ADVActions(this);

    // Support
    private readonly InputActionMap m_Support;
    private ISupportActions m_SupportActionsCallbackInterface;
    private readonly InputAction m_Support_SummonA;
    private readonly InputAction m_Support_SummonB;
    public struct SupportActions
    {
        private @PlayerInputActions m_Wrapper;
        public SupportActions(@PlayerInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @SummonA => m_Wrapper.m_Support_SummonA;
        public InputAction @SummonB => m_Wrapper.m_Support_SummonB;
        public InputActionMap Get() { return m_Wrapper.m_Support; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(SupportActions set) { return set.Get(); }
        public void SetCallbacks(ISupportActions instance)
        {
            if (m_Wrapper.m_SupportActionsCallbackInterface != null)
            {
                @SummonA.started -= m_Wrapper.m_SupportActionsCallbackInterface.OnSummonA;
                @SummonA.performed -= m_Wrapper.m_SupportActionsCallbackInterface.OnSummonA;
                @SummonA.canceled -= m_Wrapper.m_SupportActionsCallbackInterface.OnSummonA;
                @SummonB.started -= m_Wrapper.m_SupportActionsCallbackInterface.OnSummonB;
                @SummonB.performed -= m_Wrapper.m_SupportActionsCallbackInterface.OnSummonB;
                @SummonB.canceled -= m_Wrapper.m_SupportActionsCallbackInterface.OnSummonB;
            }
            m_Wrapper.m_SupportActionsCallbackInterface = instance;
            if (instance != null)
            {
                @SummonA.started += instance.OnSummonA;
                @SummonA.performed += instance.OnSummonA;
                @SummonA.canceled += instance.OnSummonA;
                @SummonB.started += instance.OnSummonB;
                @SummonB.performed += instance.OnSummonB;
                @SummonB.canceled += instance.OnSummonB;
            }
        }
    }
    public SupportActions @Support => new SupportActions(this);
    public interface IPlayerActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
        void OnAttack(InputAction.CallbackContext context);
        void OnPistol(InputAction.CallbackContext context);
        void OnAction(InputAction.CallbackContext context);
        void OnAbilitySave(InputAction.CallbackContext context);
        void OnAbilityPlay(InputAction.CallbackContext context);
    }
    public interface IADVActions
    {
        void OnAdvance(InputAction.CallbackContext context);
        void OnStartDemo(InputAction.CallbackContext context);
    }
    public interface ISupportActions
    {
        void OnSummonA(InputAction.CallbackContext context);
        void OnSummonB(InputAction.CallbackContext context);
    }
}
