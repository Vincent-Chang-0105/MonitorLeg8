using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputSystem : PersistentSingleton<InputSystem> {
    public enum ActionMap
    {
        Player,
        UI
    }

    //Debug Mode
    [SerializeField] private bool isDebug = false;
    //Unity Action Events
    public event UnityAction<Vector2> MoveEvent;
    public event UnityAction<Vector2> LookEvent;
    public event UnityAction<bool> SprintEvent;
    public event UnityAction OpenMenuEvent;

    private ActionMap _currentActionMap = ActionMap.Player;
    public ActionMap CurrentActionMap => _currentActionMap;

    // Cursor settings
    [Header("Cursor Settings")]
    public int cursorState;

    // Other settings
    [Header("Settings")]
    public bool enableLookInput = true;
    public bool enableMoveInput = true; 

    private void Start()
    {
        // Set initial cursor state
        SetCursorState(true);
    
    }

    public void SetCursorState(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;

        cursorState = (int)Cursor.lockState;
    }

    public void OnMove(InputValue value)
	{
        if(enableMoveInput)
        {
		    MoveEvent?.Invoke(value.Get<Vector2>());
        }
	}

    public void OnLook(InputValue value)
	{
		if(enableLookInput)
		{
			LookEvent?.Invoke(value.Get<Vector2>());
		}
	}

    public void OnSprint(InputValue value)
    {
        SprintEvent?.Invoke(value.isPressed);
    }

    public void OnOpenMenu(InputValue value)
    {
        if(value.isPressed)
        {
            OpenMenuEvent?.Invoke();
        }
    }

    #region Debug
    public PlayerInput playerInput;

    void OnEnable()
    {
        // var actionMap = playerInput.actions.FindActionMap("Player");

        // if(isDebug)
        // {
        //     foreach (var action in actionMap.actions)
        //     {
        //         action.performed += ctx => Debug.Log($"Performed: {action.name} Value: {ctx.ReadValueAsObject()}");
        //         action.started += ctx => Debug.Log($"Started: {action.name}");
        //         action.canceled += ctx => Debug.Log($"Canceled: {action.name}");
        //     }
        // }
    }

    void OnDisable()
    {
        // var actionMap = playerInput.actions.FindActionMap("Player");

        // if(isDebug)
        // {
        //     foreach (var action in actionMap.actions)
        //     {
        //         action.performed -= ctx => Debug.Log($"Performed: {action.name} Value: {ctx.ReadValueAsObject()}");
        //         action.started -= ctx => Debug.Log($"Started: {action.name}");
        //         action.canceled -= ctx => Debug.Log($"Canceled: {action.name}");
        //     }
        // }

    }
    #endregion
}