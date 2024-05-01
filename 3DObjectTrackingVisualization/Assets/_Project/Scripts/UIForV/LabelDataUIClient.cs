using TMPro;
using UnityEngine;

namespace UIForV
{
    public class LabelDataUIClient : MonoBehaviour
    {
        public TextMeshProUGUI label;
    
        public void SetLabelData(string data)
        {
            label.text += "\n \n";
            label.text += data;
        }
    }
}
