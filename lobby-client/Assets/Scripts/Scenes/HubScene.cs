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

        // Client instance send add follow

    }

    public void OnClickRemoveFollow(string username, string discriminator)
    {
        // Client instance send remove follow
    }

    #endregion
}