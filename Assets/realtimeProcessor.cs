﻿using UnityEngine;
using System.Collections;
public class realtimeProcessor : MonoBehaviour {
	AudioSource thisAudioSource;
	AudioClip soundGrain;
	public int samplerate = 44100;
	public float frequency = 440;

	int currentLoopStart;
	int currentLoopEnd;

	int playHeadPosition;

    private float loopStartSliderValue = 0.0f;
	private float loopEndSliderValue = 0.3f;
    private float loopLengthSliderValue = 0f;
	//private float playHeaderSliderValue=0.0f;

    float[] originalSamples;
	void Start() {
		thisAudioSource = GetComponent<AudioSource> ();
		originalSamples = new float[thisAudioSource.clip.samples * thisAudioSource.clip.channels];
		playHeadPosition = 0;
		currentLoopStart = 0044;
		currentLoopEnd = 2044;
		thisAudioSource.clip.GetData (originalSamples, 0);

	}

    void OnGUI(){
        loopStartSliderValue = GUI.HorizontalSlider(new Rect(25, 25, 500, 30), loopStartSliderValue, 0.0f, 1.0f);
		//loopEndSliderValue = GUI.HorizontalSlider(new Rect(25, 50, 500, 30),Mathf.Max(loopStartSliderValue,loopEndSliderValue), 0.0f, 1.0f);
		/*playHeaderSliderValue =*/ GUI.HorizontalSlider(new Rect(25, 75, 500, 30),((playHeadPosition*2.00f) / originalSamples.Length ), 0.0f, 1.0f);
        loopLengthSliderValue = GUI.HorizontalSlider(new Rect(25, 100, 500, 30), loopLengthSliderValue, 0.0f, 1.0f);
        setGrainLength(Mathf.FloorToInt(loopLengthSliderValue * originalSamples.Length / 2));

        //currentLoopStart = Mathf.FloorToInt (loopStartSliderValue * originalSamples.Length / 2);
        //currentLoopEnd = Mathf.FloorToInt (loopEndSliderValue * originalSamples.Length / 2);
        setGrainPosition(Mathf.FloorToInt(loopStartSliderValue * originalSamples.Length / 2));

        GUI.TextArea (new Rect (25, 125, 500, 100), "star:"+currentLoopStart+"\nend:"+currentLoopEnd+"\npos:"+playHeadPosition+"\nLen:"+(originalSamples.Length/2));
    }
    int getNextZeroCrossing(int sample) {
        //search for next zero crossing from sample
        int zeroCrossingFound = -1;
        int searchPoint = sample;
        while (zeroCrossingFound == -1)
        {
            //we need to waste one index to make zero crossing search easier, thus count before
            searchPoint++;
            if (originalSamples[searchPoint] == 0)
            {
                //condition 0 that indicats zero crossing: the sample is zero
                zeroCrossingFound = searchPoint;
                return zeroCrossingFound;
            }
            else if (originalSamples[searchPoint - 1] < 0 && originalSamples[searchPoint] > 0)
            {
                //condition 1 that indicats zero crossing: the sample crosses the zero
                zeroCrossingFound = searchPoint;
                return zeroCrossingFound;
            }
        }
        return searchPoint;
    }
    void setGrainPosition(int sample) {
        currentLoopStart = getNextZeroCrossing(sample);
    }
    void setGrainLength(int slen) {

        currentLoopEnd = getNextZeroCrossing(currentLoopStart + slen);
    }

    void OnAudioFilterRead (float[] data, int channels) {
		for (int a = 0; a < data.Length; a+=2) {
            if (playHeadPosition > currentLoopEnd){
                playHeadPosition = currentLoopStart;
            }
            int sampleposition = playHeadPosition * 2;
            //clamp
            if (sampleposition > originalSamples.Length){
                sampleposition = originalSamples.Length;
            }
            data[a] = originalSamples[sampleposition ];
			data [a+1] = originalSamples[sampleposition +1];
            playHeadPosition++;
        }

	}
}