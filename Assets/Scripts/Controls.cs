// GENERATED AUTOMATICALLY FROM 'Assets/Controls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Reactics.Core
{
    public class @Controls : IInputActionCollection, IDisposable
    {
        public InputActionAsset asset { get; }
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
                },
                {
                    ""name"": ""Select Tile"",
                    ""type"": ""Button"",
                    ""id"": ""79d2aada-7297-4954-960b-bdd7959dd46b"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Cancel Tile"",
                    ""type"": ""Button"",
                    ""id"": ""f91943f7-4b0c-4d8c-98ad-59541922d39b"",
                    ""expectedControlType"": """",
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
                },
                {
                    ""name"": """",
                    ""id"": ""0c167144-af5c-4bea-8379-555e5cc7fb13"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Select Tile"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f8dd7e6a-2243-4811-9cef-c4c533821119"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Cancel Tile"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Command Controls"",
            ""id"": ""e8bda7b3-d13a-4cab-8558-18646d3c8a9e"",
            ""actions"": [
                {
                    ""name"": ""Select Action"",
                    ""type"": ""Button"",
                    ""id"": ""8d43677c-8bfd-4094-ba71-e607e0c1f774"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Cancel Action"",
                    ""type"": ""Button"",
                    ""id"": ""52e2496c-b937-4e20-b252-56c869d39c87"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Menu Movement"",
                    ""type"": ""Value"",
                    ""id"": ""2e7f531c-48ca-4a77-9bef-6190f8fd3ee5"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""93a1f3b7-df19-455d-9a40-e6b891fee339"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Select Action"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f4472526-84fa-4fec-ba8b-cab975c7f2c5"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Cancel Action"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""48225e54-0aae-4a18-bb7c-02f68eedde3d"",
                    ""path"": ""<Gamepad>/dpad"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Menu Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Menu Controls"",
            ""id"": ""5703d1d3-8315-4905-9332-c9b7d4b21314"",
            ""actions"": [
                {
                    ""name"": ""Navigation"",
                    ""type"": ""Value"",
                    ""id"": ""7f6a1313-255a-4c17-bba4-8cf826deb3ac"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Select"",
                    ""type"": ""Button"",
                    ""id"": ""4174d118-1ea2-4ccd-b3f8-f9eb32741c65"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Back"",
                    ""type"": ""Button"",
                    ""id"": ""525a8958-619f-4610-9424-0e7fc321749e"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""Navigation"",
                    ""id"": ""36ce958d-476f-4a05-aecc-ebab98f81abd"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigation"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""1e35a539-e855-4804-91e4-0f575e1556fb"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Navigation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""6332ec96-66c2-48ff-a94f-6590ea4b2a4f"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""8ad35434-3a05-4f1f-abdf-dca7be57ccb5"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""ebd727b1-b74c-43d3-8d85-cb2931ea13bf"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Navigation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""4b006ba7-5a9f-4a3a-b21c-190bdd0bffde"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Navigation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""44316646-7eb7-49c2-87f0-80936ddf7166"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Navigation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a83a4b48-2846-4901-b68f-bff84dcbf70f"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Select"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a55bd7ea-8676-43aa-93a5-6e51baddc87e"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Select"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""86fc312c-c31e-4cfa-ac5a-03067708eb73"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Select"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4fc228e6-3b3b-484f-9705-f62d529748ca"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Back"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""63b3e0c6-43fa-484c-bcae-232dd5648b41"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Back"",
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
            m_BattleControls_SelectTile = m_BattleControls.FindAction("Select Tile", throwIfNotFound: true);
            m_BattleControls_CancelTile = m_BattleControls.FindAction("Cancel Tile", throwIfNotFound: true);
            // Command Controls
            m_CommandControls = asset.FindActionMap("Command Controls", throwIfNotFound: true);
            m_CommandControls_SelectAction = m_CommandControls.FindAction("Select Action", throwIfNotFound: true);
            m_CommandControls_CancelAction = m_CommandControls.FindAction("Cancel Action", throwIfNotFound: true);
            m_CommandControls_MenuMovement = m_CommandControls.FindAction("Menu Movement", throwIfNotFound: true);
            // Menu Controls
            m_MenuControls = asset.FindActionMap("Menu Controls", throwIfNotFound: true);
            m_MenuControls_Navigation = m_MenuControls.FindAction("Navigation", throwIfNotFound: true);
            m_MenuControls_Select = m_MenuControls.FindAction("Select", throwIfNotFound: true);
            m_MenuControls_Back = m_MenuControls.FindAction("Back", throwIfNotFound: true);
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
        private readonly InputAction m_BattleControls_SelectTile;
        private readonly InputAction m_BattleControls_CancelTile;
        public struct BattleControlsActions
        {
            private @Controls m_Wrapper;
            public BattleControlsActions(@Controls wrapper) { m_Wrapper = wrapper; }
            public InputAction @Camera => m_Wrapper.m_BattleControls_Camera;
            public InputAction @Hover => m_Wrapper.m_BattleControls_Hover;
            public InputAction @CameraZoom => m_Wrapper.m_BattleControls_CameraZoom;
            public InputAction @TileMovement => m_Wrapper.m_BattleControls_TileMovement;
            public InputAction @SelectTile => m_Wrapper.m_BattleControls_SelectTile;
            public InputAction @CancelTile => m_Wrapper.m_BattleControls_CancelTile;
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
                    @SelectTile.started -= m_Wrapper.m_BattleControlsActionsCallbackInterface.OnSelectTile;
                    @SelectTile.performed -= m_Wrapper.m_BattleControlsActionsCallbackInterface.OnSelectTile;
                    @SelectTile.canceled -= m_Wrapper.m_BattleControlsActionsCallbackInterface.OnSelectTile;
                    @CancelTile.started -= m_Wrapper.m_BattleControlsActionsCallbackInterface.OnCancelTile;
                    @CancelTile.performed -= m_Wrapper.m_BattleControlsActionsCallbackInterface.OnCancelTile;
                    @CancelTile.canceled -= m_Wrapper.m_BattleControlsActionsCallbackInterface.OnCancelTile;
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
                    @SelectTile.started += instance.OnSelectTile;
                    @SelectTile.performed += instance.OnSelectTile;
                    @SelectTile.canceled += instance.OnSelectTile;
                    @CancelTile.started += instance.OnCancelTile;
                    @CancelTile.performed += instance.OnCancelTile;
                    @CancelTile.canceled += instance.OnCancelTile;
                }
            }
        }
        public BattleControlsActions @BattleControls => new BattleControlsActions(this);

        // Command Controls
        private readonly InputActionMap m_CommandControls;
        private ICommandControlsActions m_CommandControlsActionsCallbackInterface;
        private readonly InputAction m_CommandControls_SelectAction;
        private readonly InputAction m_CommandControls_CancelAction;
        private readonly InputAction m_CommandControls_MenuMovement;
        public struct CommandControlsActions
        {
            private @Controls m_Wrapper;
            public CommandControlsActions(@Controls wrapper) { m_Wrapper = wrapper; }
            public InputAction @SelectAction => m_Wrapper.m_CommandControls_SelectAction;
            public InputAction @CancelAction => m_Wrapper.m_CommandControls_CancelAction;
            public InputAction @MenuMovement => m_Wrapper.m_CommandControls_MenuMovement;
            public InputActionMap Get() { return m_Wrapper.m_CommandControls; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(CommandControlsActions set) { return set.Get(); }
            public void SetCallbacks(ICommandControlsActions instance)
            {
                if (m_Wrapper.m_CommandControlsActionsCallbackInterface != null)
                {
                    @SelectAction.started -= m_Wrapper.m_CommandControlsActionsCallbackInterface.OnSelectAction;
                    @SelectAction.performed -= m_Wrapper.m_CommandControlsActionsCallbackInterface.OnSelectAction;
                    @SelectAction.canceled -= m_Wrapper.m_CommandControlsActionsCallbackInterface.OnSelectAction;
                    @CancelAction.started -= m_Wrapper.m_CommandControlsActionsCallbackInterface.OnCancelAction;
                    @CancelAction.performed -= m_Wrapper.m_CommandControlsActionsCallbackInterface.OnCancelAction;
                    @CancelAction.canceled -= m_Wrapper.m_CommandControlsActionsCallbackInterface.OnCancelAction;
                    @MenuMovement.started -= m_Wrapper.m_CommandControlsActionsCallbackInterface.OnMenuMovement;
                    @MenuMovement.performed -= m_Wrapper.m_CommandControlsActionsCallbackInterface.OnMenuMovement;
                    @MenuMovement.canceled -= m_Wrapper.m_CommandControlsActionsCallbackInterface.OnMenuMovement;
                }
                m_Wrapper.m_CommandControlsActionsCallbackInterface = instance;
                if (instance != null)
                {
                    @SelectAction.started += instance.OnSelectAction;
                    @SelectAction.performed += instance.OnSelectAction;
                    @SelectAction.canceled += instance.OnSelectAction;
                    @CancelAction.started += instance.OnCancelAction;
                    @CancelAction.performed += instance.OnCancelAction;
                    @CancelAction.canceled += instance.OnCancelAction;
                    @MenuMovement.started += instance.OnMenuMovement;
                    @MenuMovement.performed += instance.OnMenuMovement;
                    @MenuMovement.canceled += instance.OnMenuMovement;
                }
            }
        }
        public CommandControlsActions @CommandControls => new CommandControlsActions(this);

        // Menu Controls
        private readonly InputActionMap m_MenuControls;
        private IMenuControlsActions m_MenuControlsActionsCallbackInterface;
        private readonly InputAction m_MenuControls_Navigation;
        private readonly InputAction m_MenuControls_Select;
        private readonly InputAction m_MenuControls_Back;
        public struct MenuControlsActions
        {
            private @Controls m_Wrapper;
            public MenuControlsActions(@Controls wrapper) { m_Wrapper = wrapper; }
            public InputAction @Navigation => m_Wrapper.m_MenuControls_Navigation;
            public InputAction @Select => m_Wrapper.m_MenuControls_Select;
            public InputAction @Back => m_Wrapper.m_MenuControls_Back;
            public InputActionMap Get() { return m_Wrapper.m_MenuControls; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(MenuControlsActions set) { return set.Get(); }
            public void SetCallbacks(IMenuControlsActions instance)
            {
                if (m_Wrapper.m_MenuControlsActionsCallbackInterface != null)
                {
                    @Navigation.started -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnNavigation;
                    @Navigation.performed -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnNavigation;
                    @Navigation.canceled -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnNavigation;
                    @Select.started -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnSelect;
                    @Select.performed -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnSelect;
                    @Select.canceled -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnSelect;
                    @Back.started -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnBack;
                    @Back.performed -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnBack;
                    @Back.canceled -= m_Wrapper.m_MenuControlsActionsCallbackInterface.OnBack;
                }
                m_Wrapper.m_MenuControlsActionsCallbackInterface = instance;
                if (instance != null)
                {
                    @Navigation.started += instance.OnNavigation;
                    @Navigation.performed += instance.OnNavigation;
                    @Navigation.canceled += instance.OnNavigation;
                    @Select.started += instance.OnSelect;
                    @Select.performed += instance.OnSelect;
                    @Select.canceled += instance.OnSelect;
                    @Back.started += instance.OnBack;
                    @Back.performed += instance.OnBack;
                    @Back.canceled += instance.OnBack;
                }
            }
        }
        public MenuControlsActions @MenuControls => new MenuControlsActions(this);
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
            void OnSelectTile(InputAction.CallbackContext context);
            void OnCancelTile(InputAction.CallbackContext context);
        }
        public interface ICommandControlsActions
        {
            void OnSelectAction(InputAction.CallbackContext context);
            void OnCancelAction(InputAction.CallbackContext context);
            void OnMenuMovement(InputAction.CallbackContext context);
        }
        public interface IMenuControlsActions
        {
            void OnNavigation(InputAction.CallbackContext context);
            void OnSelect(InputAction.CallbackContext context);
            void OnBack(InputAction.CallbackContext context);
        }
    }
}
