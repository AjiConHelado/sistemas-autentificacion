using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Linq;
using System;

namespace a
{
    public class AuthHandler : MonoBehaviour
    {
        [SerializeField] TMP_InputField userNameInputField, passwordInputField;
        public string apiUrl = "https://sid-restapi.onrender.com/api/";

        string token;
        string username;

        [Header("Score list references")]
        [SerializeField] TextMeshProUGUI[] scoreUsernames;
        [SerializeField] TextMeshProUGUI[] scoreValues;

        [Header("Update score reference")]
        [SerializeField] TMP_InputField scoreInputField;

        [Header("Panel references")]
        [SerializeField] GameObject authPanel;
        [SerializeField] GameObject clientPanel;

        [Header("Other references")]
        [SerializeField] TextMeshProUGUI usernameField;

        void Start()
        {
            token = PlayerPrefs.GetString("Token");
            username = PlayerPrefs.GetString("Username");

            print("Token:" +token + "User:" + username);

            List<UserJson> lista = new List<UserJson>();
            List<UserJson> listaOrdenada = lista.OrderByDescending(u => u.data.score).ToList<UserJson>();

            if (string.IsNullOrEmpty(token)) 
            {
                authPanel.SetActive(true);
                clientPanel.SetActive(false);
                Debug.Log("No hay token");
            }
            else
            {
              
                Debug.Log(token);
                Debug.Log(usernameField);
                StartCoroutine(SendProfile());
            }
        }

        public void SignUp()
        {
            UserJson authData = new UserJson();
            authData.username = userNameInputField.text;
            authData.password = passwordInputField.text;

            string json = JsonUtility.ToJson(authData);

            StartCoroutine(SendRegister(json));
        }

        public void Login()
        {
            UserJson authData = new UserJson();
            authData.username = userNameInputField.text;
            authData.password = passwordInputField.text;

            string json = JsonUtility.ToJson(authData);

            StartCoroutine(SendLogin(json));
        }
        public void UpdateUserScore()
        {
            UserJson user = new UserJson();
            user.username = username;

            if (int.TryParse(scoreInputField.text, out _))
            {
                user.data.score = int.Parse(scoreInputField.text);
            }

            string postData = JsonUtility.ToJson(user);
            Debug.Log(postData);
            StartCoroutine(UpdateScore(postData));
        }

        IEnumerator SendRegister(string json)
        {
            UnityWebRequest request = UnityWebRequest.Put(apiUrl+"usuarios", json);
            request.SetRequestHeader("Content-Type", "application/json");
            request.method = "POST";
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("NETWORK ERROR" + request.error);
            }

            else
            {
                Debug.Log(request.downloadHandler.text);
                if (request.responseCode == 200)
                {
                    AuthJson data = JsonUtility.FromJson<AuthJson>(request.downloadHandler.text);
                    Debug.Log("Usuario registrado con el ID " + data.usuario._id);
                }
                else
                {
                    Debug.Log(request.error);
                }
            }
        }

        IEnumerator SendLogin(string json)
        {
            UnityWebRequest request = UnityWebRequest.Put(apiUrl+"auth/login", json);
            request.SetRequestHeader("Content-Type", "application/json");
            request.method = "POST";
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("NETWORK ERROR" + request.error);
            }

            else
            {
                Debug.Log(request.downloadHandler.text);
                if (request.responseCode == 200)
                {
                    AuthJson data = JsonUtility.FromJson<AuthJson>(request.downloadHandler.text);

                    token = data.token;
                    username = data.usuario.username;

                    PlayerPrefs.SetString("Token", token);
                    PlayerPrefs.SetString("Username", username);

                    Debug.Log("Inicio de sesion con el usuario " + data.usuario.username + " y su token " + data.token);

                    clientPanel.SetActive(true);
                    authPanel.SetActive(false);

                    scoreInputField.text = data.data.score.ToString();
                    usernameField.text = data.usuario.username;

                    StartCoroutine(RetrieveAndSetScores());
                }
                else
                {
                    Debug.Log(request.error);
                }
            }
        }
        IEnumerator SendProfile()
        {
            UnityWebRequest request = UnityWebRequest.Get(apiUrl + "usuarios/" + username);
            request.SetRequestHeader("x-token", token);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("NETWORK ERROR :" + request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);

                if (request.responseCode == 200)
                {

                    AuthJson data = JsonUtility.FromJson<AuthJson>(request.downloadHandler.text);

                    Debug.Log(data.usuario.username + " Sigue con la sesion inciada");

                    usernameField.text = data.usuario.username;
                    scoreInputField.text = data.usuario.data.score.ToString();

                    StartCoroutine(RetrieveAndSetScores());
                    authPanel.SetActive(false);
                    clientPanel.SetActive(true);
                }
                else
                {
                    string mensaje = "Status :" + request.responseCode;
                    mensaje += "\ncontent-type:" + request.GetResponseHeader("content-type");
                    mensaje += "\nError :" + request.error;
                    //Error.text = "Error : El usuario anterior a cerrado seccion ";
                    //StartCoroutine(Mensaje());
                    Debug.Log(mensaje);
                }

            }
        }
        IEnumerator RetrieveAndSetScores()
        {
            UnityWebRequest request = UnityWebRequest.Get(apiUrl+"usuarios");
            request.SetRequestHeader("x-token", token);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("NETWORK ERROR :" + request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);

                if (request.responseCode == 200)
                {
                    Userlist jsonList = JsonUtility.FromJson<Userlist>(request.downloadHandler.text);
                    Debug.Log(jsonList.usuarios.Count);

                    foreach (UserJson userJson in jsonList.usuarios)
                    {
                        Debug.Log(userJson.username);
                    }

                    List<UserJson> lista = jsonList.usuarios;
                    List<UserJson> listaOrdenada = lista.OrderByDescending(u => u.data.score).ToList<UserJson>();

                    int len = scoreUsernames.Length;
                    for (int i = 0; i < len; i++)
                    {
                        scoreUsernames[i].text = listaOrdenada[i].username;
                        scoreValues[i].text = listaOrdenada[i].data.score.ToString();
                    }
                }
                else
                {
                    string mensaje = "Status :" + request.responseCode;
                    mensaje += "\ncontent-type:" + request.GetResponseHeader("content-type");
                    mensaje += "\nError :" + request.error;
                    Debug.Log(mensaje);
                }
            }
        }

        IEnumerator UpdateScore(string postData)
        {
            UnityWebRequest www = UnityWebRequest.Put(apiUrl+"usuarios", postData);

            www.method = "PATCH";
            www.SetRequestHeader("x-token", token);
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                //index.SetActive(false);
                //login.SetActive(true);
                Debug.Log("NETWORK ERROR :" + www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);

                if (www.responseCode == 200)
                {

                    AuthJson jsonData = JsonUtility.FromJson<AuthJson>(www.downloadHandler.text);
                    StartCoroutine(RetrieveAndSetScores());
                    Debug.Log(jsonData.usuario.username + " se actualizo " + jsonData.usuario.data.score);
                }
                else
                {
                    string mensaje = "Status :" + www.responseCode;
                    mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                    mensaje += "\nError :" + www.error;
                    Debug.Log(mensaje);
                }

            }
        }

      
    }

    [System.Serializable]
    public class UserJson
    {
        public string _id;
        public string username;
        public string password;

        public UserData data;

        public UserJson()
        {
            data = new UserData();
        }
        public UserJson(string username, string password)
        {
            this.username = username;
            this.password = password;
            data = new UserData();
        }
    }

    [System.Serializable]
    public class UserData
    {
        public int score;
    }

    [System.Serializable]
    public class AuthJson
    {
        public UserJson usuario;
        public UserData data;
        public string token;
    }

    [System.Serializable]
    public class Userlist
    {
        public List<UserJson> usuarios;
    }
}
