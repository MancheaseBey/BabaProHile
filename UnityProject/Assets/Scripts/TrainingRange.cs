using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingRange : MonoBehaviour
{
    [System.Serializable] class TargetData { public Transform root; public Text label; public float phase; }
    readonly List<TargetData> targets = new List<TargetData>();
    Camera cam; Canvas canvas; Text status; Slider rangeSlider, fovSlider; Toggle espToggle;
    float elapsed;

    void Start()
    {
        Application.targetFrameRate = 60;
        BuildWorld(); BuildUI();
    }

    void BuildWorld()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Training Ground"; ground.transform.localScale = new Vector3(5,1,5);
        SetColor(ground, new Color(.12f,.18f,.22f));
        var light = new GameObject("Training Light").AddComponent<Light>();
        light.type = LightType.Directional; light.intensity = 1.2f; light.transform.rotation = Quaternion.Euler(50,-30,0);
        var cameraObject = new GameObject("Training Camera");
        cam = cameraObject.AddComponent<Camera>(); cam.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0,2,-10);
        cameraObject.transform.rotation = Quaternion.Euler(8,0,0);
        for (int i=0;i<6;i++)
        {
            var root = new GameObject("Training Target "+(i+1));
            root.transform.position = new Vector3((i-2.5f)*2.2f,0,Random.Range(8f,22f));
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.transform.SetParent(root.transform,false); body.transform.localPosition=Vector3.up;
            SetColor(body, new Color(.8f,.28f,.22f));
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.transform.SetParent(root.transform,false); head.transform.localPosition=new Vector3(0,2.05f,0); head.transform.localScale=Vector3.one*.45f;
            SetColor(head,new Color(.95f,.72f,.55f));
            targets.Add(new TargetData{root=root.transform,phase=i*.8f});
        }
        // Opaque practice wall: overlay labels remain visible as a deliberate ESP teaching demonstration.
        var wall=GameObject.CreatePrimitive(PrimitiveType.Cube); wall.name="Occlusion Wall";
        wall.transform.position=new Vector3(0,1.4f,7); wall.transform.localScale=new Vector3(7,2.8f,.35f);
        SetColor(wall,new Color(.28f,.32f,.36f));
    }

    void BuildUI()
    {
        var eventSystem=new GameObject("EventSystem",typeof(UnityEngine.EventSystems.EventSystem),typeof(UnityEngine.EventSystems.StandaloneInputModule));
        var canvasObject=new GameObject("Training UI",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        canvas=canvasObject.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1080,1920);
        var panel=MakePanel(canvas.transform,new Vector2(0,1),new Vector2(0,1),new Vector2(20,-20),new Vector2(620,440),new Color(.03f,.06f,.08f,.88f));
        MakeText(panel,"MOBİL HEDEFLEME LABORATUVARI",28,new Vector2(0,0),new Vector2(1,1),new Vector2(0,-30),new Vector2(-20,-10),TextAnchor.UpperCenter);
        espToggle=MakeToggle(panel,"ESP etiketlerini göster",new Vector2(0,1),new Vector2(1,1),new Vector2(10,-85),new Vector2(-20,55));
        MakeText(panel,"Algılama mesafesi",22,new Vector2(0,1),new Vector2(1,1),new Vector2(10,-145),new Vector2(-20,35),TextAnchor.MiddleLeft);
        rangeSlider=MakeSlider(panel,200,25,200,new Vector2(10,-195));
        MakeText(panel,"FOV açısı",22,new Vector2(0,1),new Vector2(1,1),new Vector2(10,-245),new Vector2(-20,35),TextAnchor.MiddleLeft);
        fovSlider=MakeSlider(panel,90,5,180,new Vector2(10,-295));
        var fire=MakeButton(panel,"HEDEF SEÇ / ATEŞ ET",new Vector2(10,-350),new Vector2(-20,65));
        fire.onClick.AddListener(SelectTarget);
        status=MakeText(panel,"Hazır — bu sahne bağımsız eğitim simülasyonudur.",18,new Vector2(0,0),new Vector2(1,0),new Vector2(10,15),new Vector2(-20,55),TextAnchor.MiddleLeft);
        for(int i=0;i<targets.Count;i++)
            targets[i].label=MakeText(canvas.transform,"",20,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(220,55),TextAnchor.MiddleCenter);
    }

    void Update()
    {
        elapsed+=Time.deltaTime;
        foreach(var t in targets)
        {
            if(t.root==null) continue;
            var p=t.root.position; p.x += Mathf.Sin(elapsed+t.phase)*.004f; t.root.position=p;
            Vector3 screen=cam.WorldToScreenPoint(t.root.position+Vector3.up*2.7f);
            bool inRange=Vector3.Distance(cam.transform.position,t.root.position)<=rangeSlider.value;
            bool visible=screen.z>0 && inRange && espToggle.isOn;
            t.label.gameObject.SetActive(visible);
            if(visible) t.label.rectTransform.position=new Vector3(screen.x,screen.y,0);
            if(visible) t.label.text="HEDEF  •  "+Mathf.RoundToInt(Vector3.Distance(cam.transform.position,t.root.position))+" m";
        }
    }

    void SelectTarget()
    {
        Transform best=null; float bestAngle=fovSlider.value*.5f; float maxDistance=rangeSlider.value;
        foreach(var t in targets)
        {
            float d=Vector3.Distance(cam.transform.position,t.root.position); if(d>maxDistance) continue;
            float a=Vector3.Angle(cam.transform.forward,(t.root.position+Vector3.up-cameraPositionOffset())-cam.transform.position);
            if(a<=bestAngle){best=t.root;bestAngle=a;}
        }
        status.text=best==null?"Bu FOV ve mesafede hedef yok.":"Seçilen eğitim hedefi: "+best.name+"  |  Açı: "+bestAngle.ToString("0.0")+"°";
    }
    Vector3 cameraPositionOffset(){return Vector3.zero;}
    static void SetColor(GameObject go,Color c){var r=go.GetComponent<Renderer>(); if(r!=null) r.material.color=c;}
    RectTransform MakePanel(Transform parent,Vector2 amin,Vector2 amax,Vector2 pos,Vector2 size,Color color){var g=new GameObject("Panel",typeof(RectTransform),typeof(Image));g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.pivot=new Vector2(0,1);r.anchoredPosition=pos;r.sizeDelta=size;g.GetComponent<Image>().color=color;return r;}
    Text MakeText(Transform parent,string value,int size,Vector2 amin,Vector2 amax,Vector2 pos,Vector2 dimensions,TextAnchor anchor){var g=new GameObject("Text",typeof(RectTransform),typeof(Text));g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.anchoredPosition=pos;r.sizeDelta=dimensions;var t=g.GetComponent<Text>();t.text=value;t.font=Resources.GetBuiltinResource<Font>("Arial.ttf");t.fontSize=size;t.color=Color.white;t.alignment=anchor;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Overflow;return t;}
    Toggle MakeToggle(Transform p,string label,Vector2 amin,Vector2 amax,Vector2 pos,Vector2 size){var g=new GameObject("ESP Toggle",typeof(RectTransform),typeof(Toggle));g.transform.SetParent(p,false);var r=g.GetComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.pivot=new Vector2(0,1);r.anchoredPosition=pos;r.sizeDelta=size;var bg=new GameObject("Background",typeof(RectTransform),typeof(Image));bg.transform.SetParent(g.transform,false);var br=bg.GetComponent<RectTransform>();br.anchorMin=new Vector2(0,0);br.anchorMax=new Vector2(0,1);br.sizeDelta=new Vector2(40,0);bg.GetComponent<Image>().color=Color.gray;var check=new GameObject("Check",typeof(RectTransform),typeof(Image));check.transform.SetParent(bg.transform,false);check.GetComponent<RectTransform>().sizeDelta=new Vector2(24,24);check.GetComponent<Image>().color=Color.green;var t=MakeText(g.transform,label,22,new Vector2(0,0),new Vector2(1,1),new Vector2(55,0),new Vector2(-55,0),TextAnchor.MiddleLeft);var toggle=g.GetComponent<Toggle>();toggle.targetGraphic=bg.GetComponent<Image>();toggle.graphic=check.GetComponent<Image>();toggle.isOn=true;return toggle;}
    Slider MakeSlider(Transform p,float value,float min,float max,Vector2 pos){var g=new GameObject("Slider",typeof(RectTransform),typeof(Slider));g.transform.SetParent(p,false);var r=g.GetComponent<RectTransform>();r.anchorMin=new Vector2(0,1);r.anchorMax=new Vector2(1,1);r.pivot=new Vector2(0,1);r.anchoredPosition=pos;r.sizeDelta=new Vector2(-30,35);var bg=new GameObject("Background",typeof(RectTransform),typeof(Image));bg.transform.SetParent(g.transform,false);bg.GetComponent<RectTransform>().anchorMin=Vector2.zero;bg.GetComponent<RectTransform>().anchorMax=Vector2.one;bg.GetComponent<Image>().color=new Color(.2f,.25f,.3f);var fill=new GameObject("Fill",typeof(RectTransform),typeof(Image));fill.transform.SetParent(bg.transform,false);fill.GetComponent<RectTransform>().anchorMin=Vector2.zero;fill.GetComponent<RectTransform>().anchorMax=Vector2.one;fill.GetComponent<Image>().color=new Color(.1f,.8f,.65f);var handle=new GameObject("Handle",typeof(RectTransform),typeof(Image));handle.transform.SetParent(g.transform,false);handle.GetComponent<RectTransform>().sizeDelta=new Vector2(35,35);handle.GetComponent<Image>().color=Color.white;var s=g.GetComponent<Slider>();s.fillRect=fill.GetComponent<RectTransform>();s.handleRect=handle.GetComponent<RectTransform>();s.targetGraphic=handle.GetComponent<Image>();s.minValue=min;s.maxValue=max;s.value=value;s.direction=Slider.Direction.LeftToRight;return s;}
    Button MakeButton(Transform p,string label,Vector2 pos,Vector2 size){var g=new GameObject("Action Button",typeof(RectTransform),typeof(Image),typeof(Button));g.transform.SetParent(p,false);var r=g.GetComponent<RectTransform>();r.anchorMin=new Vector2(0,1);r.anchorMax=new Vector2(1,1);r.pivot=new Vector2(0,1);r.anchoredPosition=pos;r.sizeDelta=size;g.GetComponent<Image>().color=new Color(.1f,.75f,.58f);MakeText(g.transform,label,23,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,TextAnchor.MiddleCenter);return g.GetComponent<Button>();}
}
