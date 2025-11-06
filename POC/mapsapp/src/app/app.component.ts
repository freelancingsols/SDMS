import { Component } from '@angular/core';
import { AfterViewInit, ViewChild, ElementRef } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})

export class AppComponent implements AfterViewInit {
  @ViewChild('mapContainer', { static: false }) gmap: ElementRef;
  ngOnInit() {

  }
  constructor() {


  }
  googlemapsloaded: false
  title = 'mapsapp';
  key = 'AIzaSyBBkoh0Sbwiq92dvl1ViKCF7ZVKlqybuqs';
  static pr: Promise<any>
  mapInitializer() {
    let map: google.maps.Map;
    let lat = 19.543360;
    let lng = 74.245049;

    let coordinates = new google.maps.LatLng(lat, lng);
    let coordinates1 = new google.maps.LatLng(lat+0.11, lng+0.11);
    let coordinates2 = new google.maps.LatLng(19.583349,74.986240);
    let coordinates3 = new google.maps.LatLng(lat+0.20, lng+0.15);
    let coordinates4 = new google.maps.LatLng(lat+0.22, lng+0.15);
    let coordinates5 = new google.maps.LatLng(lat+0.30, lng+0.30);
    let mapOptions: google.maps.MapOptions = {
      center: coordinates,
      zoom: 8
    };

    let marker = new google.maps.Marker({
      position: coordinates,
      map: map,
      crossOnDrag:false
    });
    let marker1 = new google.maps.Marker({
      position: coordinates5,
      map: map,
      crossOnDrag:false
    });
    map = new google.maps.Map(this.gmap.nativeElement,
      mapOptions);
    marker.setMap(map);
    marker1.setMap(map);
    let polil=new google.maps.Polyline({path:[coordinates,coordinates1,coordinates2,coordinates3,coordinates4,coordinates5]});
    polil.setVisible(true);
    polil.setMap(map);
  }
  ngAfterViewInit() {
    var pr = new Promise((resolve) => {

      // Create the script tag, set the appropriate attributes
      var script = document.createElement('script');
      script.src = 'https://maps.googleapis.com/maps/api/js?key=AIzaSyBBkoh0Sbwiq92dvl1ViKCF7ZVKlqybuqs&callback=initMap';
      script.defer = false;
      // Attach your callback function to the `window` object
      (<any>window).initMap = function () {
        // JS API is loaded and available
      };
      script.onload = () => {
        resolve();
      };
      // Append the 'script' element to 'head'
      document.head.appendChild(script);

      /* setTimeout(() => {
        resolve();
      }, 200);
       */
    });

    pr.then(() => {
      this.mapInitializer();
    });
  }

}

