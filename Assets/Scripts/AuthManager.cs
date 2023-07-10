using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;
public class AuthManager : MonoBehaviour
{
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text confirmLoginText;
    public TMP_Text warningLoginText;
    [Header("Register")]
    public TMP_InputField userRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordConfirmRegisterField;
    public TMP_Text warningRegisterText;

    private void Awake() 
    {
        //Check that all necesary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => 
        {
            dependencyStatus = task.Result;
            if(dependencyStatus == DependencyStatus.Available)
            {
                //If they are available Initilize Firebase
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });   
    }
    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        auth = FirebaseAuth.DefaultInstance;
    }
    public void LoginButtom()
    {
        StartCoroutine(Login(emailLoginField.text,  passwordLoginField.text));
    }
    public void RegisterButtom()
    {
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, userRegisterField.text));
    }
    private IEnumerator Login(string _email, string _password)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        yield return new WaitUntil(predicate: () => loginTask.IsCompleted);
        if(loginTask.Exception != null)
        {
            //If there are errors, handle them
            Debug.LogWarning(message: $"Failed to register task with {loginTask.Exception}");
            FirebaseException firebaseEx = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
            string message = "Login Failed!";
            switch(errorCode)
            {
                case AuthError.MissingEmail:
                message = "Missing Email";
                break;
                case AuthError.MissingPassword:
                message = "Missing Password";
                break;
                case AuthError.WrongPassword:
                message = "Wrong Password";
                break;
                case AuthError.InvalidEmail:
                message = "Invalid Email";
                break;
                case AuthError.UserNotFound:
                message = "User Not Found";
                break;
            }
            warningLoginText.text = message;
        }
        else
        {
            //User is now logged in
            //Now get the result
            user = loginTask.Result.User;
            Debug.LogFormat("User signed in succesfully: {0} ({1})", user.DisplayName, user.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Logged in!";
        }
    }
    private IEnumerator Register(string _email, string _password, string _userName)
    {
        if(_userName == "")
        {
            warningRegisterText.text = "Missing Username";
        }
        else if(passwordRegisterField.text != passwordConfirmRegisterField.text)
        {
            warningRegisterText.text = "Passwords does not match!";
        }
        else
        {
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            yield return new WaitUntil(predicate: () => registerTask.IsCompleted);
            if(registerTask.Exception != null)
            {
                //If there are errors, handle them
                Debug.LogWarning(message: $"Failed to register task with {registerTask.Exception}");
                FirebaseException firebaseEx = registerTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                string message = "Register Failed!";
                switch(errorCode)
                {
                    case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                    case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                    case AuthError.WeakPassword:
                    message = "Weak Paswword";
                    break;
                    case AuthError.EmailAlreadyInUse:
                    message = "Email Already In Use";
                    break;
                }
                warningRegisterText.text = message;
            }
            else
            {
                //User has now been created
                //Now get result
                user = registerTask.Result.User;
                if(user != null)
                {
                    UserProfile profile = new UserProfile{DisplayName = _userName};
                    var profileTask = user.UpdateUserProfileAsync(profile);
                    yield return new WaitUntil(predicate: () => profileTask.IsCompleted);
                    if (profileTask.Exception != null)
                    {
                        Debug.LogWarning(message: $"Failed to register task with {profileTask.Exception}");
                        FirebaseException firebaseEx = profileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        warningRegisterText.text = "Username Set Failed!";
                    }
                    else
                    {
                        //Username is now set
                        //Now return to login screen
                        UIManager.instance.LoginScreen();
                        warningRegisterText.text = "";
                    }
                }
            }
        }
    }

}
