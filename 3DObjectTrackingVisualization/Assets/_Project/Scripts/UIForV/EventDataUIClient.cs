using TMPro;
using UnityEngine;

namespace UIForV
{
    public class EventDataUIClient : MonoBehaviour
    {
        public TextMeshProUGUI eventText;
    
        public void SetDefaultEventData(string eventName, string data)
        {
            eventText.text += "\n \n";
            eventText.text += "<#008000>"+ eventName + "</color>" + "\n";
            eventText.text += data;
        }
        
        public void SetGreenEventData(string eventName, string data)
        {
            eventText.text += "\n \n";
            eventText.text += "<#008000>"+ eventName + "</color>" + "\n";
            eventText.text += "<#008000>"+ data + "</color>";
        }
        
        public void SetYellowEventData(string eventName, string data)
        {
            eventText.text += "\n \n";
            eventText.text += "<#008000>"+ eventName + "</color>" + "\n";
            eventText.text += "<#FFFF00>"+ data + "</color>";
        }
        
        public void SetRedEventData(string eventName, string data)
        {
            eventText.text +=  "\n \n";
            eventText.text += "<#008000>"+ eventName + "</color>" + "\n";
            eventText.text += "<#FF0000>"+ data + "</color>";
        }
    }
}
