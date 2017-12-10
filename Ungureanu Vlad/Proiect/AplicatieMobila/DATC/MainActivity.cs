﻿using Android.App;
using Android.Widget;
using Android.OS;
using Android.Gms.Maps;
using System;
using Android.Gms.Maps.Model;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Timers;
using System.Threading;

namespace DATC
{
    [Activity(Label = "Irrigation Master", MainLauncher = true)]
    public class MainActivity : Activity, IOnMapReadyCallback
    {
        static HttpClient client = new HttpClient();
        static GoogleMap mMap;
        Button btnTemp, btnUmid, btnPres;
        public static System.Timers.Timer timpActualizare = new System.Timers.Timer(90000);
        public void OnMapReady(GoogleMap googleMap)
        {
            LatLng cameralatLng = new LatLng(45.740363, 21.244295);
            mMap = googleMap;
            mMap.MapType = GoogleMap.MapTypeSatellite;
            CameraUpdate camera = CameraUpdateFactory.NewLatLngZoom(cameralatLng, 18);
            mMap.MoveCamera(camera);
            mMap.MarkerClick += MMap_MarkerClick;
            mMap.MapLongClick += MMap_MapLongClick;
            for (int index = 0; index < Helper.listaHeatMapTemp.Count; index++)
            {
                Helper.DesenarePoligon(mMap, new LatLng(double.Parse(Helper.listaHeatMapTemp[index].LatA), double.Parse(Helper.listaHeatMapTemp[index].LngA)), new LatLng(double.Parse(Helper.listaHeatMapTemp[index].LatB), double.Parse(Helper.listaHeatMapTemp[index].LngB)), new LatLng(double.Parse(Helper.listaHeatMapTemp[index].LatC), double.Parse(Helper.listaHeatMapTemp[index].LngC)), new LatLng(double.Parse(Helper.listaHeatMapTemp[index].LatD), double.Parse(Helper.listaHeatMapTemp[index].LngD)), Helper.listaHeatMapTemp[index].Culoare);
            }
        }

        private void MMap_MapLongClick(object sender, GoogleMap.MapLongClickEventArgs e)
        {
            Helper.markersVisible = !Helper.markersVisible;
            if (Helper.markersVisible == true)
            {
                for (int index = 0; index < Helper.listaSenzori.Count; index++)
                    Helper.AdaugareMarker(mMap, Helper.listaSenzori[index].Coordonate, "Senzor " + Helper.listaSenzori[index].Idsenzor);
            }
            else
            {
                mMap.Clear();
                for (int index = 0; index < Helper.listaHeatMapTemp.Count; index++)
                {
                    Helper.DesenarePoligon(mMap, new LatLng(double.Parse(Helper.listaHeatMapTemp[index].LatA), double.Parse(Helper.listaHeatMapTemp[index].LngA)), new LatLng(double.Parse(Helper.listaHeatMapTemp[index].LatB), double.Parse(Helper.listaHeatMapTemp[index].LngB)), new LatLng(double.Parse(Helper.listaHeatMapTemp[index].LatC), double.Parse(Helper.listaHeatMapTemp[index].LngC)), new LatLng(double.Parse(Helper.listaHeatMapTemp[index].LatD), double.Parse(Helper.listaHeatMapTemp[index].LngD)), Helper.listaHeatMapTemp[index].Culoare);
                }
            }
        }

        private void MMap_MarkerClick(object sender, GoogleMap.MarkerClickEventArgs e)
        {
            Helper.senzorCurent = e.Marker.Title;
            StartActivity(typeof(SenzorActivity));
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (Helper.vizualizareaCurenta == Helper.Vizualizare.Temperatura)
            {

            }
            else if (Helper.vizualizareaCurenta == Helper.Vizualizare.Umiditate)
            {

            }
            else
            {

            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);
            btnTemp = FindViewById<Button>(Resource.Id.btnTemp);
            btnUmid = FindViewById<Button>(Resource.Id.btnUmid);
            btnPres = FindViewById<Button>(Resource.Id.btnPres);
            SetupMap();
            btnTemp.Click += BtnTemp_Click;
            btnUmid.Click += BtnUmid_Click;
            btnPres.Click += BtnPres_Click;
            timpActualizare.Elapsed += TimpActualizare_Elapsed;
            //Preluare lista senzori
            try
            {
                client.DefaultRequestHeaders.Add("Accept", "application/hal+json");
                var Home = "http://datcapitmv.azurewebsites.net/api/values";
                var response = client.GetAsync(Home).Result;
                string data = response.Content.ReadAsStringAsync().Result;
                Helper.listaSenzori = JsonConvert.DeserializeObject<List<Senzor>>(data);
            }
            catch (Exception e) { }
            for (int index = 0; index < Helper.listaSenzori.Count; index++)
            {
                Helper.listaSenzori[index].Coordonate = new LatLng(double.Parse(Helper.listaSenzori[index].Latitudine), double.Parse(Helper.listaSenzori[index].Longitudine));
            }
            //Preluare HeatMap temperatura
            try
            {
                var Home = "http://datcapitmv.azurewebsites.net/api/Temperatura";
                var response = client.GetAsync(Home).Result;
                string data = response.Content.ReadAsStringAsync().Result;
                Helper.listaHeatMapTemp = JsonConvert.DeserializeObject<List<HeatMap>>(data);
            }
            catch (Exception e) { }
            timpActualizare.Start();
        }
        private void TimpActualizare_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Helper.listaHeatMapTemp.Clear();
                var Home = "http://datcapitmv.azurewebsites.net/api/Temperatura";
                var response = client.GetAsync(Home).Result;
                string data = response.Content.ReadAsStringAsync().Result;
                Helper.listaHeatMapTemp = JsonConvert.DeserializeObject<List<HeatMap>>(data);
            }
            catch (Exception ex) { }
            RunOnUiThread(() =>
            {
                mMap.Clear();
                if (Helper.vizualizareaCurenta == Helper.Vizualizare.Temperatura)
                {
                    for (int index = 0; index < Helper.listaHeatMapTemp.Count; index++)
                    {
                        Helper.DesenarePoligon(mMap, new LatLng(double.Parse(Helper.listaHeatMapTemp[index].LatA), double.Parse(Helper.listaHeatMapTemp[index].LngA)), new LatLng(double.Parse(Helper.listaHeatMapTemp[index].LatB), double.Parse(Helper.listaHeatMapTemp[index].LngB)), new LatLng(double.Parse(Helper.listaHeatMapTemp[index].LatC), double.Parse(Helper.listaHeatMapTemp[index].LngC)), new LatLng(double.Parse(Helper.listaHeatMapTemp[index].LatD), double.Parse(Helper.listaHeatMapTemp[index].LngD)), Helper.listaHeatMapTemp[index].Culoare);
                    }
                }
                else if (Helper.vizualizareaCurenta == Helper.Vizualizare.Umiditate)
                {

                }
                else
                {

                }
            });
        }

        private void BtnPres_Click(object sender, EventArgs e)
        {
            Helper.vizualizareaCurenta = Helper.Vizualizare.Presiune;
        }

        private void BtnUmid_Click(object sender, EventArgs e)
        {
            Helper.vizualizareaCurenta = Helper.Vizualizare.Umiditate;
        }

        private void BtnTemp_Click(object sender, EventArgs e)
        {
            Helper.vizualizareaCurenta = Helper.Vizualizare.Temperatura;

        }

        private void SetupMap()
        {
            if (mMap == null)
            {
                FragmentManager.FindFragmentById<MapFragment>(Resource.Id.map).GetMapAsync(this);
            }
        }
    }
}

