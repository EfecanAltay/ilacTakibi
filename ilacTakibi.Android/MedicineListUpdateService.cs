using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using ilacTakibi.ViewModels;
using System;
using System.Timers;

namespace ilacTakibi.Droid
{
    [Service(Enabled = true, Exported = true, Name = "com.bilbest.ilacTakibi.Droid.MedicineListUpdateService")]
    public class MedicineListUpdateService : Service
    {
        static readonly string TAG = typeof(MedicineListUpdateService).FullName;
        MedicineListPageViewModel viewModel = App.referenceViewModel;
        public IBinder Binder { get; private set; }
        public Timer timer;

        public override void OnCreate()
        {
            base.OnCreate();
        }

        public override IBinder OnBind(Intent intent)
        {
            this.Binder = new MedicineControlBinder(this);
            viewModel = App.referenceViewModel;
            timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            return this.Binder;
        }

        //public override StartCommandResult OnStartCommand(Android.Content.Intent intent, StartCommandFlags flags, int startId)
        //{
        //    return StartCommandResult.NotSticky;
        //}

        private async void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (DateTime.Now.Second == 0)
            {
                viewModel = App.referenceViewModel;
                await viewModel.FetchMedicineList();
                await viewModel.UpdateLiveMedicineList();
                Log.Debug(TAG, "Updated MedicineList");
                viewModel.NotifyWhenNotUsedMedicinesCommand.Execute(null);
            }
        }

        public override bool OnUnbind(Intent intent)
        {
            // This method is optional to implement
            Log.Debug(TAG, "OnUnbind");
            return base.OnUnbind(intent);
        }

        public override void OnDestroy()
        {
            // This method is optional to implement
            Log.Debug(TAG, "OnDestroy");
            //timer.Stop();
            //timer.Dispose();
            //timer = null;
            Binder = null;
            //viewModel = null;
            //timestamper = null;
            base.OnDestroy();
        }
    }

    public class MedicineControlBinder : Binder
    {
        public MedicineControlBinder(MedicineListUpdateService service)
        {
            this.Service = service;
        }

        public MedicineListUpdateService Service { get; private set; }
    }
}