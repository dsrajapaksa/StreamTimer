﻿using MvvmHelpers;
using MyStreamTimer.Shared.Helpers;
using MyStreamTimer.Shared.Interfaces;
using MyStreamTimer.Shared.Model;
using System;
using System.IO;
using System.Timers;
using System.Windows.Input;

// kymphillpotts cheered 150 March 9th 2019

namespace MyStreamTimer.Shared.ViewModel
{
    public class TimerViewModel : BaseViewModel
    {


        DateTime startTime;
        DateTime endTime;
        bool currentIsDown;
        int currentMinutes;
        string currentFinished, currentOutput, currentFileName;
        readonly Timer timer;
        string identifier;

        Settings settings;

        public TimerViewModel(string id)
        {
            identifier = id;
            settings = new Settings(id);

            switch(identifier)
            {
                case Constants.Countdown:
                    IsDown = true;
                    break;
                case Constants.Countup:
                    IsDown = false;
                    break;
                case Constants.Giveaway:
                    IsDown = true;
                    break;
            }

            StartStopTimerCommand = new Command(ExecuteStartStopTimerCommand);
            CopyFilePathCommand = new Command(ExecuteCopyFilePathCommand);
            timer = new Timer(250);
            timer.Elapsed += TimerElapsed;
            timer.AutoReset = true;

            if (AutoStart)
                ExecuteStartStopTimerCommand();
        }

        bool isDown = true;
        public bool IsDown
        {
            get => isDown;
            set => SetProperty(ref isDown, value);
        }

        public int Minutes
        {
            get => settings.Minutes;
            set
            {
                settings.Minutes = value;
                OnPropertyChanged(nameof(Minutes));
            }
        }

        public string Output
        {
            get => settings.Output;
            set
            {
                settings.Output = value;
                OnPropertyChanged(nameof(Output));
            }
        }

        public bool AutoStart
        {
            get => settings.AutoStart;
            set
            {
                settings.AutoStart = value;
                OnPropertyChanged(nameof(AutoStart));
            }
        }

        public string Finish
        {
            get => settings.Finish;
            set
            {
                settings.Finish = value;
                OnPropertyChanged(nameof(Finish));
            }
        }
        public string FileName
        {
            get => settings.FileName;
            set
            {
                settings.FileName = value;
                OnPropertyChanged(nameof(FileName));
            }
        }

        string countdownOutput;
        public string CountdownOutput
        {
            get => countdownOutput;
            set => SetProperty(ref countdownOutput, value);
        }

        string startStop = "Start";
        public string StartStop
        {
            get => startStop;
            set => SetProperty(ref startStop, value);
        }

        public ICommand CopyFilePathCommand { get; set; }

        void ExecuteCopyFilePathCommand()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            folder = Path.Combine(folder, "MyStreamTimer");

            var clipboard = ServiceContainer.Resolve<IClipboard>();

            clipboard?.CopyToClipboard(folder);
        }

        public ICommand StartStopTimerCommand { get; set; }

        void ExecuteStartStopTimerCommand()
        {
            try
            {
                string.Format(Output, TimeSpan.FromSeconds(5));
            }
            catch
            {
                CountdownOutput = @"Invalid time format. Use {0:hh\:mm\:ss}";
                return;
            }

            try
            {
                currentFileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                currentFileName = Path.Combine(currentFileName, "MyStreamTimer");
                if (!Directory.Exists(currentFileName))
                    Directory.CreateDirectory(currentFileName);

                currentFileName = Path.Combine(currentFileName, FileName);
            }
            catch (Exception ex)
            {
                CountdownOutput = ex.Message;
                return;
            }

            IsBusy = !IsBusy;
            StartStop = IsBusy ? "Stop" : "Start";

            timer.Enabled = IsBusy;

            if (!IsBusy)
                return;

            currentFinished = Finish;
            currentIsDown = IsDown;
            currentMinutes = Minutes;
            currentOutput = Output;

            startTime = DateTime.Now;
            endTime = DateTime.Now.AddMinutes(currentMinutes);
            TimerElapsed(null, null);
        }
        void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            var now = DateTime.Now;
            if (currentIsDown)
            {
                if (now >= endTime)
                {
                    ExecuteStartStopTimerCommand();
                    CountdownOutput = currentFinished;
                }
                else
                {
                    var elapsedTime = endTime - DateTime.Now;
                    CountdownOutput = string.Format(currentOutput, elapsedTime);
                }
            }
            else
            {
                var elapsedTime = DateTime.Now - startTime;
                CountdownOutput = string.Format(currentOutput, elapsedTime);
            }

            WriteTimeToDisk();
        }

        void WriteTimeToDisk()
        {
            try
            {
                File.WriteAllText(currentFileName, CountdownOutput);
            }
            catch (Exception ex)
            {
                CountdownOutput = ex.Message;
            }
        }
    }
}
