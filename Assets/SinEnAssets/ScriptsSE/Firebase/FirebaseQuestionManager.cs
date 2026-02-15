/*
* Author: Kwek Sin En
* Date: 10/02/2026
* Description: Fetches custom teacher-created questions from Firebase
* Falls back to procedurally generated questions if no custom questions available
* Mixes Firebase and generated questions to avoid repetition
*/
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using System.Linq;

public class FirebaseQuestionManager : MonoBehaviour
{
    /// <summary>
    /// Represents question data for Firebase, 
    /// including the question text, correct answer, wrong answers, and creation timestamp.
    /// </summary>
    [System.Serializable]
    public class FirebaseQuestionData
    {
        public string question;
        public float correctAnswer;
        public List<float> wrongAnswers;
        public long createdAt;
    }

    [Header("References")]
    [SerializeField] private MathQuestionGenerator mathQuestionGenerator;

    [Header("Scene Configuration")]
    [SerializeField] private string level1SceneName = "Level1";
    [SerializeField] private string level2SceneName = "BasicScene_SELevel";
    [SerializeField] private string level3SceneName = "WeiChengScene";

    [Header("Question Management")]
    [SerializeField] private bool useFirebaseQuestions = true;
    [SerializeField] private int minQuestionsPerLevel = 6; // Minimum questions needed per level (matches enemy count)
    [SerializeField] private float firebaseQuestionChance = 0.5f; // Base chance when pool is small

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    private DatabaseReference databaseReference;
    private Dictionary<string, List<FirebaseQuestionData>> cachedQuestions = new Dictionary<string, List<FirebaseQuestionData>>();
    private Dictionary<string, List<int>> usedQuestionIndices = new Dictionary<string, List<int>>();
    private bool isFirebaseInitialized = false;
    private string currentSceneName;
    private string currentLevelKey;

    void Start()
    {
        currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        currentLevelKey = GetLevelKeyFromScene(currentSceneName);

        if (mathQuestionGenerator == null)
        {
            mathQuestionGenerator = GetComponent<MathQuestionGenerator>();
        }

        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                isFirebaseInitialized = true;

                if (debugMode) Debug.Log("Firebase initialized successfully");

                // Load questions for current level
                LoadQuestionsForLevel(currentLevelKey);
            }
            else
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {dependencyStatus}");
                isFirebaseInitialized = false;
            }
        });
    }

    /// <summary>
    /// Returns the level key corresponding to the specified scene name.
    /// </summary>
    /// <param name="sceneName">The name of the scene to map to a level key.</param>
    /// <returns>The level key associated with the given scene name, or "level1" if the scene name is not recognized.</returns>
    private string GetLevelKeyFromScene(string sceneName)
    {
        if (sceneName == level1SceneName) return "level1";
        if (sceneName == level2SceneName) return "level2";
        if (sceneName == level3SceneName) return "level3";

        Debug.LogWarning($"Scene '{sceneName}' not recognized, defaulting to level1");
        return "level1";
    }

    /// <summary>
    /// Loads custom questions for the specified level from Firebase and caches them for later use. Falls back to
    /// generated questions if Firebase is not initialized or no custom questions are found.
    /// </summary>
    /// <param name="levelKey">The key identifying the level for which to load questions.</param>
    private void LoadQuestionsForLevel(string levelKey)
    {
        if (!isFirebaseInitialized)
        {
            Debug.LogWarning("Firebase not initialized, using generated questions");
            return;
        }

        databaseReference.Child("customQuestions").Child(levelKey).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Failed to load questions from Firebase: {task.Exception}");
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                List<FirebaseQuestionData> questions = new List<FirebaseQuestionData>();

                if (snapshot.Exists && snapshot.ChildrenCount > 0)
                {
                    foreach (DataSnapshot questionSnapshot in snapshot.Children)
                    {
                        try
                        {
                            FirebaseQuestionData question = new FirebaseQuestionData();
                            question.question = questionSnapshot.Child("question").Value.ToString();
                            question.correctAnswer = float.Parse(questionSnapshot.Child("correctAnswer").Value.ToString());
                            question.createdAt = long.Parse(questionSnapshot.Child("createdAt").Value.ToString());

                            question.wrongAnswers = new List<float>();
                            DataSnapshot wrongAnswersSnapshot = questionSnapshot.Child("wrongAnswers");
                            foreach (DataSnapshot wrongAnswer in wrongAnswersSnapshot.Children)
                            {
                                question.wrongAnswers.Add(float.Parse(wrongAnswer.Value.ToString()));
                            }

                            questions.Add(question);
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"Error parsing question: {e.Message}");
                        }
                    }

                    cachedQuestions[levelKey] = questions;
                    usedQuestionIndices[levelKey] = new List<int>();

                    if (debugMode)
                        Debug.Log($"Loaded {questions.Count} custom questions for {levelKey}");
                }
                else
                {
                    if (debugMode)
                        Debug.Log($"No custom questions found for {levelKey}, will use generated questions");
                }
            }
        });
    }

    /// <summary>
    /// Gets a random question - either from Firebase or generated
    /// Intelligently mixes both sources to ensure at least 6 unique questions
    /// </summary>
    public MathQuestionGenerator.QuestionData GetQuestion()
    {
        // If Firebase questions not available or disabled, return null (use generated)
        if (!useFirebaseQuestions ||
            !cachedQuestions.ContainsKey(currentLevelKey) ||
            cachedQuestions[currentLevelKey].Count == 0)
        {
            if (debugMode) Debug.Log("No Firebase questions available, using generated question");
            return null;
        }

        int firebaseQuestionCount = cachedQuestions[currentLevelKey].Count;

        // Calculate how many Firebase questions to use based on pool size
        // If we have 6+ questions, use all Firebase
        // If we have less, mix with generated to reach 6 unique questions

        if (firebaseQuestionCount >= minQuestionsPerLevel)
        {
            // Enough Firebase questions - use them exclusively
            if (debugMode) Debug.Log($"Using Firebase question (pool of {firebaseQuestionCount})");
            return GetFirebaseQuestion();
        }
        else
        {
            // Not enough Firebase questions - need to mix with generated
            // Calculate ratio: if we have 2 Firebase questions out of 6 needed,
            // use Firebase 2/6 = 33% of the time
            float usageRatio = (float)firebaseQuestionCount / minQuestionsPerLevel;

            // Apply base chance modifier (allows tuning)
            float adjustedChance = usageRatio * firebaseQuestionChance;

            float randomValue = Random.value;

            if (randomValue < adjustedChance)
            {
                if (debugMode)
                    Debug.Log($"Using Firebase question ({firebaseQuestionCount}/{minQuestionsPerLevel} questions, {adjustedChance * 100:F0}% chance)");
                return GetFirebaseQuestion();
            }
            else
            {
                if (debugMode)
                    Debug.Log($"Using generated question for variety ({firebaseQuestionCount}/{minQuestionsPerLevel} Firebase questions available)");
                return null; // Use generated question
            }
        }
    }

    /// <summary>
    /// Retrieves a random unused question from the cached Firebase questions for the current level, resetting the used
    /// pool if all questions have been used.
    /// </summary>
    /// <returns>A MathQuestionGenerator.QuestionData object representing the selected question.</returns>
    private MathQuestionGenerator.QuestionData GetFirebaseQuestion()
    {
        List<FirebaseQuestionData> questions = cachedQuestions[currentLevelKey];
        List<int> usedIndices = usedQuestionIndices[currentLevelKey];

        int selectedIndex;

        // If we've used all questions, reset the used list
        if (usedIndices.Count >= questions.Count)
        {
            usedIndices.Clear();
            if (debugMode) Debug.Log("All Firebase questions used, resetting pool");
        }

        // Find an unused question
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < questions.Count; i++)
        {
            if (!usedIndices.Contains(i))
            {
                availableIndices.Add(i);
            }
        }

        // Select random from available
        if (availableIndices.Count > 0)
        {
            selectedIndex = availableIndices[Random.Range(0, availableIndices.Count)];
        }
        else
        {
            // Fallback (shouldn't happen due to reset above)
            selectedIndex = Random.Range(0, questions.Count);
        }

        // Mark as used
        usedIndices.Add(selectedIndex);

        FirebaseQuestionData fbQuestion = questions[selectedIndex];

        // Convert Firebase question to MathQuestionGenerator format
        MathQuestionGenerator.QuestionData questionData = new MathQuestionGenerator.QuestionData();
        questionData.question = fbQuestion.question;
        questionData.correctAnswer = fbQuestion.correctAnswer;
        questionData.wrongAnswers = fbQuestion.wrongAnswers.ToArray();

        if (debugMode)
            Debug.Log($"Using Firebase question {selectedIndex + 1}/{questions.Count}: {questionData.question}");

        return questionData;
    }

    /// <summary>
    /// Check if Firebase questions are available for current level
    /// </summary>
    public bool HasFirebaseQuestions()
    {
        return cachedQuestions.ContainsKey(currentLevelKey) &&
               cachedQuestions[currentLevelKey].Count > 0;
    }

    /// <summary>
    /// Get count of available Firebase questions
    /// </summary>
    public int GetFirebaseQuestionCount()
    {
        if (cachedQuestions.ContainsKey(currentLevelKey))
            return cachedQuestions[currentLevelKey].Count;
        return 0;
    }

    /// <summary>
    /// Refresh questions from Firebase
    /// </summary>
    public void RefreshQuestions()
    {
        if (isFirebaseInitialized)
        {
            LoadQuestionsForLevel(currentLevelKey);
        }
    }

    /// <summary>
    /// Reset the used questions tracker (for testing or level restart)
    /// </summary>
    public void ResetUsedQuestions()
    {
        if (usedQuestionIndices.ContainsKey(currentLevelKey))
        {
            usedQuestionIndices[currentLevelKey].Clear();
            if (debugMode) Debug.Log("Manually reset used questions pool");
        }
    }
}