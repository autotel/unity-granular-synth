#pragma strict

var clip : AudioClip;

var playbackSpeed = 1;
var grainSize = 1000;
var grainStep = 1;

var guiPlaybackSpeed = 1.0;
var guiGrainSize = 1000.0;
var guiGrainStep = 1.0;
var guiGrainPosition = 0;

private var sampleLength : int;
private var samples : float[];

private var position = 0;
private var interval = 0;
//if the granule is longer than the audiobuffer,
//we will need that next buffer continues from the last
//buffering position of the sample + 1
private var bufferPan = 0;

function Awake() {
	sampleLength = clip.samples;
	samples = new float[clip.samples * clip.channels];
	clip.GetData(samples, 0);
}

function Update() {
	var cursor : PositionView = FindObjectOfType(PositionView);
	cursor.position = 1.0 / sampleLength * position;
	cursor.width = 1.0 / sampleLength * interval * playbackSpeed;
}

function OnGUI() {
	GUILayout.BeginArea(Rect(16, 16, Screen.width - 32, Screen.height - 32));
	GUILayout.FlexibleSpace();
	GUILayout.Label("Playback Speed: " + playbackSpeed);
	guiPlaybackSpeed = GUILayout.HorizontalSlider(guiPlaybackSpeed, -4.0, 4.0);
	GUILayout.FlexibleSpace();
	GUILayout.Label("Grain Size: " + grainSize);
	guiGrainSize = GUILayout.HorizontalSlider(guiGrainSize, 1, 100000.0);
	GUILayout.FlexibleSpace();
	GUILayout.Label("Grain Step: " + grainStep);
	guiGrainStep = GUILayout.HorizontalSlider(guiGrainStep, -3000.0, 3000.0);
	GUILayout.FlexibleSpace();
	GUILayout.Label("Grain position: " + position);
	guiGrainPosition = GUILayout.HorizontalSlider(guiGrainPosition, 0, 100000);
	GUILayout.FlexibleSpace();
	if (GUILayout.Button("RANDOMIZE!")) {
		guiPlaybackSpeed = Random.Range(-2.0, 2.0);
		guiGrainSize = Random.Range(200.0, 1000.0);
		guiGrainStep = Random.Range(-1500.0, 1500.0);
	}
	GUILayout.FlexibleSpace();
	GUILayout.EndArea();
	
	playbackSpeed = Mathf.RoundToInt(guiPlaybackSpeed);
	//if (playbackSpeed == 0) playbackSpeed = 1;
	grainSize = Mathf.RoundToInt((guiGrainSize/100000.00)*(Mathf.Floor(sampleLength/2)*2));
	grainStep = Mathf.RoundToInt(guiGrainStep);
	position = 	Mathf.RoundToInt((guiGrainPosition/100000.00)*(Mathf.Floor(sampleLength/2)));
}

function OnAudioFilterRead(data : float[], channels : int) {
    for (var i = 0; i < data.Length; i += 2) {
    	
    	data[i] = samples[(position * 2)%sampleLength];
    	data[i + 1] = samples[(position * 2 + 1)%sampleLength];



    	//search next same voltage sample
    	var nextSameLevelSample=grainSize;
    	while(samples[(nextSameLevelSample * 2)%sampleLength ]!=samples[(nextSameLevelSample * 2)%position]){
    		nextSameLevelSample++;
    	}

        if (--interval <= 0) {
        	interval=nextSameLevelSample;
	        //interval = grainSize;
	        //position += grainStep;
        } else {
        	position += playbackSpeed;
        }
        //clamp variables
        if (position >= sampleLength/2) {
        	position = sampleLength/2 - 2;
        }
        if (position < 0) {
        	position = 0;
        }

        /*for (var cn = 0; i < channels; cn ++) {
        	//data[i] = Mathf.Sin(position/700);
	        data[i] = samples[position * channels + cn];
	        //data[i + 1] = samples[position * 2 + 1];
	    }*/
	    
    }
}