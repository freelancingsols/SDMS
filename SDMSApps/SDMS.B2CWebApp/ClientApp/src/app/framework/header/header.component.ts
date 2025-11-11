import { Component, OnInit } from '@angular/core';
declare let $: any;
declare var google: any;

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit {

  constructor() { }

  ngOnInit() {
    this.initMap();
  }
  // async ngOnInit() {
  //   $(window).scroll(function () {
  //     $('nav').toggleClass('scrolled', $(this).scrollTop() > 30);
  //   });

  //   $('.navbar .nav-item').click(function () {
  //     $('.navbar .nav-item').removeClass('active');
  //     $(this).addClass('active');
  //   });


  //   // this.cart$ = await this.cartServ.getCart();
  // }

  initMap() {
    const map = new google.maps.Map(
      document.getElementById("map") as HTMLElement,
      {
        center: { lat: -33.8688, lng: 151.2195 },
        zoom: 13,
      }
    );
    const card = document.getElementById("pac-card") as HTMLElement;
    const input = document.getElementById("pac-input") as HTMLInputElement;
  
    map.controls[google.maps.ControlPosition.TOP_RIGHT].push(card);
  
    const autocomplete = new google.maps.places.Autocomplete(input);
  
    // Bind the map's bounds (viewport) property to the autocomplete object,
    // so that the autocomplete requests use the current map bounds for the
    // bounds option in the request.
    autocomplete.bindTo("bounds", map);
  
    // Set the data fields to return when the user selects a place.
    autocomplete.setFields(["address_components", "geometry", "icon", "name"]);
  
    const infowindow = new google.maps.InfoWindow();
    const infowindowContent = document.getElementById(
      "infowindow-content"
    ) as HTMLElement;
    infowindow.setContent(infowindowContent);
    const marker = new google.maps.Marker({
      map,
      anchorPoint: new google.maps.Point(0, -29),
    });
  
    autocomplete.addListener("place_changed", () => {
      infowindow.close();
      marker.setVisible(false);
      const place = autocomplete.getPlace();
  
      if (!place.geometry) {
        // User entered the name of a Place that was not suggested and
        // pressed the Enter key, or the Place Details request failed.
        window.alert("No details available for input: '" + place.name + "'");
        return;
      }
  
      // If the place has a geometry, then present it on a map.
      if (place.geometry.viewport) {
        map.fitBounds(place.geometry.viewport);
      } else {
        map.setCenter(place.geometry.location);
        map.setZoom(17); // Why 17? Because it looks good.
      }
      marker.setPosition(place.geometry.location);
      marker.setVisible(true);
  
      let address = "";
  
      if (place.address_components) {
        address = [
          (place.address_components[0] &&
            place.address_components[0].short_name) ||
            "",
          (place.address_components[1] &&
            place.address_components[1].short_name) ||
            "",
          (place.address_components[2] &&
            place.address_components[2].short_name) ||
            "",
        ].join(" ");
      }
  
      const placeIcon = infowindowContent.querySelector('#place-icon') as HTMLImageElement;
      const placeName = infowindowContent.querySelector('#place-name') as HTMLElement;
      const placeAddress = infowindowContent.querySelector('#place-address') as HTMLElement;
      
      if (placeIcon) placeIcon.src = place.icon;
      if (placeName) placeName.textContent = place.name;
      if (placeAddress) placeAddress.textContent = address;
      infowindow.open(map, marker);
    });
  
    // Sets a listener on a radio button to change the filter type on Places
    // Autocomplete.
    function setupClickListener(id: string, types: string[]) {
      const radioButton = document.getElementById(id) as HTMLInputElement;
      radioButton.addEventListener("click", () => {
        autocomplete.setTypes(types);
      });
    }
  
    setupClickListener("changetype-all", []);
    setupClickListener("changetype-address", ["address"]);
    setupClickListener("changetype-establishment", ["establishment"]);
    setupClickListener("changetype-geocode", ["geocode"]);
  
    (document.getElementById(
      "use-strict-bounds"
    ) as HTMLInputElement).addEventListener("click", function () {
      console.log("Checkbox clicked! New state=" + this.checked);
      autocomplete.setOptions({ strictBounds: this.checked });
    });
  }

}
