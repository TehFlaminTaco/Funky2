using Funky;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;
using NAudio;
using NAudio.Wave;

namespace Funky.Libs{
    public static class LibSound{
        public static Dictionary<VarList, IWavePlayer> soundLists = new Dictionary<VarList, IWavePlayer>();
        public static VarList Generate(){
            VarList sound = new VarList();

            sound["loadSound"] = new VarFunction(dat => {
                VarList soundList = new VarList();
                IWavePlayer waveOutDevice = new WaveOutEvent();
                AudioFileReader audioFileReader = new AudioFileReader(dat.num_args[0].asString());
                waveOutDevice.Init(audioFileReader);
                soundLists[soundList] = waveOutDevice;
                bool destroyed = false;
                waveOutDevice.PlaybackStopped += (object s, NAudio.Wave.StoppedEventArgs e)=>{
                    if(destroyed)return;
                    audioFileReader.Position = 0;
                    soundList.Get("onFinished").Call(new CallData(soundList));
                };

                soundList["onFinished"] = new VarEvent("onFinished");
                soundList["play"] = new VarFunction(d=>{
                    if(destroyed) return Var.nil;
                    audioFileReader.Position = 0;
                    waveOutDevice.Play();
                    return soundList;
                });
                soundList["stop"] = new VarFunction(d=>{
                    if(destroyed) return Var.nil;
                    waveOutDevice.Stop();
                    return soundList;
                });
                soundList["setVolume"] = new VarFunction(d=>{
                    if(destroyed) return Var.nil;
                    waveOutDevice.Volume = (float)FunkyHelpers.ReadArgument(d, 0, "volume", 1.0f).asNumber();
                    return soundList;
                });
                soundList["getVolume"] = new VarFunction(d=>{
                    if(destroyed) return Var.nil;
                    return waveOutDevice.Volume;
                });
                soundList["destroy"] = new VarFunction(d=>{
                    if(destroyed) return Var.nil;
                    destroyed = true;
                    waveOutDevice.Dispose();
                    audioFileReader.Close();
                    soundLists.Remove(soundList);
                    return sound;
                });
                return soundList;
            });

            return sound;
        }
    }
}