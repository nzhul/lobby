using TMPro;
using UnityEngine;

public class HubScene : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI selfInformation;

    [SerializeField]
    private TMP_InputField addFollowInput;

    private void Start()
    {
        selfInformation.text = Client.Instance.self.Username + "#" + Client.Instance.self.Discriminator;
        Client.Instance.SendRequestFollow();
    }

    #region Button 
    public void OnClickAddFollow()
    {
        string usernameDiscriminator = addFollowInput.text;

        if (!Utility.IsUsernameAndDiscriminator(usernameDiscriminator) && !Utility.IsEmail(usernameDiscriminator))
        {
            Debug.Log("Invalid format");
            return;
        }

        Client.Instance.SendAddFollow(usernameDiscriminator);
    }

    public void OnClickRemoveFollow(string username, string discriminator)
    {
        Client.Instance.SendRemoveFollow(username + "#" + discriminator); 
    }

    #endregion
}