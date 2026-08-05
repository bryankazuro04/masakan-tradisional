using System;
using System.Collections.Generic;
using UnityEngine;
using MasakanTradisional.Data;

namespace MasakanTradisional.Core.FSM
{
    /// <summary>
    /// Base interface for states without entry parameters.
    /// </summary>
    public interface IGameState
    {
        void Enter();
        void Update();
        void Exit();
    }

    /// <summary>
    /// Interface for states that receive data payload upon entering (e.g., RecipeData).
    /// </summary>
    public interface IStateWithPayload<T> : IGameState
    {
        void Enter(T payload);
    }

    /// <summary>
    /// Central Finite State Machine handling transitions between Recipe Selection,
    /// Kitchen Prep, and active Cooking states.
    /// </summary>
    public class GameStateMachine : MonoBehaviour
    {
        public static GameStateMachine Instance { get; private set; }

        public IGameState CurrentState { get; private set; }
        public RecipeData CurrentSelectedRecipe { get; private set; }

        // System Events
        public event Action<IGameState> OnStateChanged;
        public event Action<RecipeData> OnRecipeSelected;

        private Dictionary<Type, IGameState> _states;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeStates();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Default initial state
            GoToRecipeSelection();
        }

        private void Update()
        {
            CurrentState?.Update();
        }

        /// <summary>
        /// Populates state dictionary with concrete instances.
        /// </summary>
        private void InitializeStates()
        {
            _states = new Dictionary<Type, IGameState>
            {
                { typeof(RecipeSelectionState), new RecipeSelectionState(this) },
                { typeof(KitchenPrepState), new KitchenPrepState(this) },
                { typeof(CookingState), new CookingState(this) }
            };
        }

        /// <summary>
        /// Transition to a state by type.
        /// </summary>
        public void ChangeState<T>() where T : IGameState
        {
            Type stateType = typeof(T);

            if (!_states.TryGetValue(stateType, out IGameState newState))
            {
                Debug.LogError($"[GameStateMachine] State '{stateType.Name}' not found!");
                return;
            }

            if (CurrentState == newState) return;

            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter();

            OnStateChanged?.Invoke(CurrentState);
            Debug.Log($"[GameStateMachine] Transitioned to State: {stateType.Name}");
        }

        /// <summary>
        /// Transition to a state by type while passing payload data.
        /// </summary>
        public void ChangeState<T, TPayload>(TPayload payload) where T : IStateWithPayload<TPayload>
        {
            Type stateType = typeof(T);

            if (!_states.TryGetValue(stateType, out IGameState state))
            {
                Debug.LogError($"[GameStateMachine] State '{stateType.Name}' not found!");
                return;
            }

            if (state is IStateWithPayload<TPayload> parameterizedState)
            {
                CurrentState?.Exit();
                CurrentState = parameterizedState;
                parameterizedState.Enter(payload);

                OnStateChanged?.Invoke(CurrentState);
                Debug.Log($"[GameStateMachine] Transitioned to State: {stateType.Name} with payload '{payload}'");
            }
        }

        /// <summary>
        /// Called when a recipe selection button is clicked on RecipeCardUI.
        /// Caches the selected recipe and transitions the FSM to KitchenPrepState.
        /// </summary>
        public void SelectRecipe(RecipeData recipeData)
        {
            if (recipeData == null)
            {
                Debug.LogWarning("[GameStateMachine] Cannot select null RecipeData!");
                return;
            }

            CurrentSelectedRecipe = recipeData;
            OnRecipeSelected?.Invoke(recipeData);

            Debug.Log($"[GameStateMachine] Recipe Selected: '{recipeData.recipeName}'. Transitioning to KitchenPrepState...");
            ChangeState<KitchenPrepState, RecipeData>(recipeData);
        }

        // Shortcut helper methods
        public void GoToRecipeSelection() => ChangeState<RecipeSelectionState>();
        public void GoToKitchenPrep(RecipeData recipeData) => SelectRecipe(recipeData);
        public void GoToCooking() => ChangeState<CookingState>();
    }

    /// <summary>
    /// Active state during main menu recipe selection.
    /// </summary>
    public class RecipeSelectionState : IGameState
    {
        private readonly GameStateMachine _stateMachine;

        public RecipeSelectionState(GameStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            Debug.Log("[RecipeSelectionState] Entered Recipe Selection screen UI.");
        }

        public void Update()
        {
            // Listen for user UI actions or idle state
        }

        public void Exit()
        {
            Debug.Log("[RecipeSelectionState] Exiting Recipe Selection phase.");
        }
    }

    /// <summary>
    /// Active state during workstation setup and ingredient preparation.
    /// </summary>
    public class KitchenPrepState : IGameState, IStateWithPayload<RecipeData>
    {
        private readonly GameStateMachine _stateMachine;
        public RecipeData CurrentRecipe { get; private set; }

        public KitchenPrepState(GameStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            Debug.Log("[KitchenPrepState] Entered Kitchen Prep(Default).");
        }

        public void Enter(RecipeData payload)
        {
            CurrentRecipe = payload;
            Debug.Log($"[KitchenPrepState] Workstation prepared for '{payload?.recipeName}'. Required Ingredients: {payload?.Ingredients.Count}, Tools: {payload?.RequiredTools.Count}.");
        }

        public void Update()
        {
            // Monitor ingredient prep conditions and player readiness
        }

        public void Exit()
        {
            Debug.Log($"[KitchenPrepState] Preparation complete for '{CurrentRecipe?.recipeName}'.");
        }
    }

    /// <summary>
    /// Active state during cooking step execution.
    /// </summary>
    public class CookingState : IGameState
    {
        private readonly GameStateMachine _stateMachine;

        public CookingState(GameStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            RecipeData activeRecipe = _stateMachine.CurrentSelectedRecipe;
            Debug.Log($"[CookingState] Cooking session initiated for '{activeRecipe?.recipeName}'. Step count: {activeRecipe?.StepCount}.");
        }

        public void Update()
        {
            // Ticks step timers, evaluates cooking temperatures, and checks step completion
        }

        public void Exit()
        {
            Debug.Log("[CookingState] Exiting Cooking phase.");
        }
    }
}