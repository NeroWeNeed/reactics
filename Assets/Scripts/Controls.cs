// GENERATED AUTOMATICALLY FROM 'Assets/Controls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @Controls : IInputActionCollection, IDisposable
{
    private InputActionAsset asset;
    public @Controls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Controls"",
    ""maps"": [
        {
            ""name"": ""Battle Controls"",
            ""id"": ""641ba7d6-dd9b-4d9e-8421-13f05f1acd79"",
            ""actions"": [
                {
                    ""name"": ""Camera"",
                    ""type"": ""Value"",
                    ""id"": ""a5648adf-58d7-47a9-8cb2-d6fd752de626"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Hover"",
                    ""type"": ""Value"",
                    ""id"": ""4dd7095c-a83e-45b1-821c-6e1a30ac0e26"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""23614920-d281-4d26-abfd-ea8203e902f8"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Camera"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""ebc0d9d4-c20c-4421-b8c8-9b9454520fcd"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""5859e703-053d-4066-bc9a-39313177f9bd"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""8c7c0f3b-57e0-477d-8c32-ca90259a7e3f"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""f1e84a3e-f1bd-48d3-9d9c-bd796c738bf3"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""7f649279-8b24-416f-9909-3139fa815f1e"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Hover"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard + Mouse"",
            ""bindingGroup"": ""Keyboard + Mouse"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Battle Controls
        m_BattleControls = asset.FindActionMap("Battle Controls", throwIfNotFound: true);
        m_BattleControls_Camera = m_BattleControls.FindAction("Camera", throwIfNotFound: true);
        m_BattleControls_Hover = m_BattleControls.FindAction("Hover", throwIfNotFound: true);
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

    // Battle Controls
    private readonly InputActionMap m_BattleControls;
    private IBattleControlsActions m_BattleControlsActionsCallbackInterface;
    private readonly InputAction m_BattleControls_Camera;
    private readonly InputAction m_BattleControls_Hover;
    public struct BattleControlsActions
    {
        private @Controls m_Wrapper;
        public BattleControlsActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Camera => m_Wrapper.m_BattleControls_Camera;
        public InputAction @Hover => m_Wrapper.m_BattleControls_Hover;
        public InputActionMap Get() { return m_Wrapper.m_BattleControls; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(BattleControlsActions set) { return set.Get(); }
        public void SetCallbacks(IBattleControlsActions instance)
        {
            if (m_Wrapper.m_BattleControlsActionsCallbackInterface != null)
            {
                @Camera.started -= m_Wrapper.m_BattleControlsActionsCallbackInterface.OnCamera;
                @Camera.performed -= m_Wrapper.m_BattleControlsActionsCallbackInterface.OnCamera;
                @Camera.canceled -= m_Wrapper.m_BattleControlsActionsCallbackInterface.OnCamera;
                @Hover.started -= m_Wrapper.m_BattleControlsActionsCallbackInterface.OnHover;
                @Hover.performed -= m_Wrapper.m_BattleControlsActionsCallbackInterface.OnHover;
                @Hover.canceled -= m_Wrapper.m_BattleControlsActionsCallbackInterface.OnHover;
            }
            m_Wrapper.m_BattleControlsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Camera.started += instance.OnCamera;
                @Camera.performed += instance.OnCamera;
                @Camera.canceled += instance.OnCamera;
                @Hover.started += instance.OnHover;
                @Hover.performed += instance.OnHover;
                @Hover.canceled += instance.OnHover;
            }
        }
    }
    public BattleControlsActions @BattleControls => new BattleControlsActions(this);
    private int m_KeyboardMouseSchemeIndex = -1;
    public InputControlScheme KeyboardMouseScheme
    {
        get
        {
            if (m_KeyboardMouseSchemeIndex == -1) m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard + Mouse");
            return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
        }
    }
    public interface IBattleControlsActions
    {
        void OnCamera(InputAction.CallbackContext context);
        void OnHover(InputAction.CallbackContext context);
    }
}
