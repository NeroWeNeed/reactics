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
                },
                {
                    ""name"": ""Camera Zoom"",
                    ""type"": ""Value"",
                    ""id"": ""19bf0dcc-b4b2-4747-a460-c858ea85bffc"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Tile Movement"",
                    ""type"": ""Value"",
                    ""id"": ""bded49ca-aeba-4234-b0f2-79ee2340e246"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector (Keyboard)"",
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
                    ""id"": ""61f33c33-f9d3-4a3e-a8b3-8c80bf9369a0"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""03851877-34d6-493b-ac66-2218776c0ed8"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Hover"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""acff6732-c684-4cc0-9fe5-9115cd12b6c3"",
                    ""path"": ""<Mouse>/scroll/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Camera Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""35ad860a-006d-4ea1-99ef-677362a477c9"",
                    ""path"": ""1DAxis(minValue=-120,maxValue=120)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Camera Zoom"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""fedbc333-c2ce-423c-ade0-ce8e6ab7026f"",
                    ""path"": ""<Gamepad>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Camera Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""3adc8d29-d601-4377-82cd-0094fdaeae0f"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Camera Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""f5af0945-cedf-4fed-9aff-9649a5a5f53c"",
                    ""path"": ""<Gamepad>/dpad"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Tile Movement"",
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
        },
        {
            ""name"": ""Gamepad"",
            ""bindingGroup"": ""Gamepad"",
            ""devices"": [
                {
                    ""devicePath"": ""<Gamepad>"",
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
        m_BattleControls_CameraZoom = m_BattleControls.FindAction("Camera Zoom", throwIfNotFound: true);
        m_BattleControls_TileMovement = m_BattleControls.FindAction("Tile Movement", throwIfNotFound: true);
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
    private readonly InputAction m_BattleControls_CameraZoom;
    private readonly InputAction m_BattleControls_TileMovement;
    public struct BattleControlsActions
    {
        private @Controls m_Wrapper;
        public BattleControlsActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Camera => m_Wrapper.m_BattleControls_Camera;
        public InputAction @Hover => m_Wrapper.m_BattleControls_Hover;
        public InputAction @CameraZoom => m_Wrapper.m_BattleControls_CameraZoom;
        public InputAction @TileMovement => m_Wrapper.m_BattleControls_TileMovement;
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
                @CameraZoom.started -= m_Wrapper.m_BattleControlsActionsCallbackInterface.OnCameraZoom;
                @CameraZoom.performed -= m_Wrapper.m_BattleControlsActionsCallbackInterface.OnCameraZoom;
                @CameraZoom.canceled -= m_Wrapper.m_BattleControlsActionsCallbackInterface.OnCameraZoom;
                @TileMovement.started -= m_Wrapper.m_BattleControlsActionsCallbackInterface.OnTileMovement;
                @TileMovement.performed -= m_Wrapper.m_BattleControlsActionsCallbackInterface.OnTileMovement;
                @TileMovement.canceled -= m_Wrapper.m_BattleControlsActionsCallbackInterface.OnTileMovement;
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
                @CameraZoom.started += instance.OnCameraZoom;
                @CameraZoom.performed += instance.OnCameraZoom;
                @CameraZoom.canceled += instance.OnCameraZoom;
                @TileMovement.started += instance.OnTileMovement;
                @TileMovement.performed += instance.OnTileMovement;
                @TileMovement.canceled += instance.OnTileMovement;
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
    private int m_GamepadSchemeIndex = -1;
    public InputControlScheme GamepadScheme
    {
        get
        {
            if (m_GamepadSchemeIndex == -1) m_GamepadSchemeIndex = asset.FindControlSchemeIndex("Gamepad");
            return asset.controlSchemes[m_GamepadSchemeIndex];
        }
    }
    public interface IBattleControlsActions
    {
        void OnCamera(InputAction.CallbackContext context);
        void OnHover(InputAction.CallbackContext context);
        void OnCameraZoom(InputAction.CallbackContext context);
        void OnTileMovement(InputAction.CallbackContext context);
    }
}
