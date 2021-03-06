﻿using UnityEngine;
using System.Collections;
public class realtimeProcessor : MonoBehaviour {
	AudioSource thisAudioSource;
	AudioClip soundGrain;
	public int samplerate = 44100;
	public float frequency = 440;
    public int pointsToAnalyze = 100;
    public int meanDifferenceAnalysisDifference = 1024;
    public float dafaultsGLength = 0.2f;
    float currentLoopScore = 0;
    int currentSelectedLoopLength = 0;
    int goneThrough = 0;
    int selectedZero = 0;

    
    int currentLoopStart;
	int currentLoopEnd;
    int currentLoopLength;

    int playHeadPosition;

    private bool initialized = false;
    private float loopStartSliderValue = 0.0f;
	private float loopEndSliderValue = 0.3f;
    private float loopLengthSliderValue = 0f;
    
    //private float playHeaderSliderValue=0.0f;

    float[] originalSamples;
	void Start() {
		thisAudioSource = GetComponent<AudioSource> ();
		originalSamples = new float[thisAudioSource.clip.samples * thisAudioSource.clip.channels];
		playHeadPosition = 0;
        //currentLoopStart = 0;
        loopLengthSliderValue = dafaultsGLength;
        //currentLoopEnd = 4342;
        thisAudioSource.clip.GetData (originalSamples, 0);
        initialized = true;

    }

    void OnGUI(){
        int margin = 25;
        int inMarginWidth = Screen.width - 2*margin;
        int cline = 0;
        loopStartSliderValue = GUI.HorizontalSlider(new Rect(margin, cline, inMarginWidth, margin), loopStartSliderValue, 0.0f, 1.0f);
        cline += margin;
		//loopEndSliderValue = GUI.HorizontalSlider(new Rect(25, 50, 500, 30),Mathf.Max(loopStartSliderValue,loopEndSliderValue), 0.0f, 1.0f);
		/*playHeaderSliderValue =*/ GUI.HorizontalSlider(new Rect(margin, cline, inMarginWidth, margin),((playHeadPosition*2.00f) / originalSamples.Length ), 0.0f, 1.0f);
        cline += margin;
        loopLengthSliderValue = GUI.HorizontalSlider(new Rect(margin, cline, inMarginWidth, margin), loopLengthSliderValue, 0.0f, 1.0f);
        cline += margin;
        GUI.HorizontalSlider(new Rect(margin, cline, inMarginWidth, margin), panLfo, -1.0f, 1.0f);
        cline += margin;

        setGrainPosition(Mathf.FloorToInt(loopStartSliderValue * originalSamples.Length / 2));
        setGrainLength(Mathf.FloorToInt(loopLengthSliderValue * originalSamples.Length / 2));

        //currentLoopStart = Mathf.FloorToInt (loopStartSliderValue * originalSamples.Length / 2);
        //currentLoopEnd = Mathf.FloorToInt (loopEndSliderValue * originalSamples.Length / 2);


        GUI.TextArea (new Rect (margin, cline, inMarginWidth, 200), 
            "star:"+currentLoopStart
            +"\nend:"+currentLoopEnd+"("+(currentLoopEnd- currentLoopStart) + ")"
            +"\npos:"+playHeadPosition
            +"\nLen:"+(originalSamples.Length/2)
            + "\n Length would be:" + currentSelectedLoopLength
            + "\n Similarity score is:" + currentLoopScore + " an "+goneThrough+"sel "+selectedZero
            + "\n panlfo"+panLfo
            );
    }
    public void setStartingPoint(float num) {
        setGrainPosition(Mathf.FloorToInt(num * originalSamples.Length*8 / 3));
    }
    int getNextPositiveZeroCrossing(int sample) {
        //search for next zero crossing from sample
        int zeroCrossingFound = -1;
        int searchPoint = sample;
        
        while (zeroCrossingFound == -1)
        {
            //we need to waste one index to make zero crossing search easier, thus count before
            searchPoint++;
            if (searchPoint >= originalSamples.Length) {
                return -1;
            }
            //if either this sample is zero or the two samples have different sign, it's a zero cross.
            //good idea sourced from https://www.dsprelated.com/showcode/179.php
            if (originalSamples[searchPoint - 1] * originalSamples[searchPoint] < 0)
            {
                zeroCrossingFound = searchPoint;
                return zeroCrossingFound;
            }
        }
        return searchPoint;
    }
    void setGrainPosition(int sample) {
        currentLoopStart = getNextPositiveZeroCrossing(sample);
    }
    void setGrainLength(int slen) {
        currentSelectedLoopLength = slen;
        goneThrough = 0;

        int[]zeroCrossingList = new int[pointsToAnalyze];
        int ZeroCrossingSeekPosition = slen+currentLoopStart;
        //list some zero crossings (the amount to list is user defined)
        for (int a = 0; a < zeroCrossingList.Length; a++) {
            zeroCrossingList[a]=getNextPositiveZeroCrossing(ZeroCrossingSeekPosition);
            //if there was a zero crossing found, next repetition we search zero crossing from this point on.
            if (zeroCrossingList[a] != -1) {
                ZeroCrossingSeekPosition = zeroCrossingList[a];
            };
        }
        //here we store the most similar starting point found so far.
        int mostSimilarStartingPointInList = 0;
        //the lower score, the more similar
        float mostSimilarStartingPointScore = 99999999;
        //now analyze each zero crossing with our starting point, to find the most similar according to mean difference
        
        //for each zero cross in the list. zeroCrossingList[b] will be an individual sample position. 
        for (int b = 0; b < zeroCrossingList.Length; b++){
            float thisDifference = 0;
            //analze now sample by sample
            for (int c=0;c < meanDifferenceAnalysisDifference; c++){
                int comparisonHeadA = currentLoopStart + c;
                int comparisonHeadB = zeroCrossingList[b] + c;
                //just make sure we are not out of range
                if (!((comparisonHeadA >= originalSamples.Length) || (comparisonHeadB >= originalSamples.Length))){
                    goneThrough++;
                    //the first points affect the most, while the last points don't affect so much. I don't know what decay curve to use 
                    float wheight = ((c*0.1f) / meanDifferenceAnalysisDifference);
                    wheight = wheight * wheight;
                    thisDifference += Mathf.Abs(originalSamples[comparisonHeadA]- originalSamples[comparisonHeadB])*wheight;
                }else{
                    c = meanDifferenceAnalysisDifference + 1;
                }
                //if we already are scoring worse than the best, skip this evaluation
                if (thisDifference > mostSimilarStartingPointScore)
                {
                    c = meanDifferenceAnalysisDifference + 1;
                }
            }
            if (thisDifference < mostSimilarStartingPointScore)
            {
                mostSimilarStartingPointScore = thisDifference;
                mostSimilarStartingPointInList = zeroCrossingList[b];
                selectedZero = b;
            }
        }
        currentLoopScore = mostSimilarStartingPointScore;
        currentLoopEnd = getNextPositiveZeroCrossing(currentLoopStart + slen);
        currentLoopLength =  currentLoopEnd- currentLoopStart;
    }
    //int nonLoopedPlayHeader = 0;
    //the realtime filter uses it to avoid stuter from the remaining gap.
    private float panLfo = 0;
    void OnAudioFilterRead (float[] data, int channels) {
        if (initialized)
        {
            for (int a = 0; a < data.Length; a += 2)
            {
                //there should be two voices that are constantly crossfading, to smoothen out the small gaps
                int relativePosition = playHeadPosition - currentLoopStart;
                panLfo = 1;// (Mathf.Sin(2f * Mathf.PI * ((relativePosition - 0.25f) / currentLoopLength)) + 1) / 2;
                if ((playHeadPosition > currentLoopEnd) || (playHeadPosition < currentLoopStart))
                {
                    playHeadPosition = currentLoopStart;
                };
                int sampleposition = playHeadPosition * 2;
                //clamp
                if (sampleposition > originalSamples.Length)
                {
                    sampleposition = originalSamples.Length;
                }
                int otherHalf = 0;
                //get the other half to blend with
                /* if (relativePosition <= (currentLoopLength / 2))
                 {
                     otherHalf = sampleposition - (currentLoopLength / 2);
                 }
                 else {
                     otherHalf = sampleposition + (currentLoopLength / 2);
                 }
                 */
                data[a] = (originalSamples[sampleposition] * panLfo);//+ (originalSamples[otherHalf] * (1-panLfo));
                data[a + 1] = (originalSamples[sampleposition + 1] * panLfo);//+ (originalSamples[otherHalf+1] * (1-panLfo));
                playHeadPosition++;

            }
        }
	}
}
