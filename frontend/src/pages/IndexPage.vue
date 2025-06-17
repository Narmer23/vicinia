<template>
  <q-page class="row">
    <!-- Map Container -->
    <div class="col-12 col-md-8">
      <div class="map-container">
        <div id="map" ref="mapContainer"></div>
        
        <!-- Search Panel -->
        <div class="search-panel">
          <q-card class="search-card">
            <q-card-section>
              <div class="text-h6">Search Address</div>
            </q-card-section>
            
            <q-card-section>
              <q-input
                v-model="searchAddress"
                label="Enter an address in Lombardy"
                outlined
                dense
                @keyup.enter="searchLocation"
              >
                <template v-slot:append>
                  <q-btn
                    round
                    dense
                    flat
                    icon="search"
                    @click="searchLocation"
                    :loading="isSearching"
                  />
                </template>
              </q-input>
            </q-card-section>

            <q-card-section>
              <div class="row q-gutter-sm">
                <q-btn
                  label="Car"
                  :color="transportationMode === 'car' ? 'primary' : 'grey'"
                  @click="transportationMode = 'car'"
                  outline
                />
                <q-btn
                  label="Walking"
                  :color="transportationMode === 'walking' ? 'primary' : 'grey'"
                  @click="transportationMode = 'walking'"
                  outline
                />
              </div>
            </q-card-section>
          </q-card>
        </div>

        <!-- Score Display -->
        <div v-if="currentScore" class="score-panel">
          <q-card class="score-card">
            <q-card-section>
              <div class="text-h4 text-center">{{ currentScore.overallScore }}/10</div>
              <div class="text-caption text-center">Overall Score</div>
            </q-card-section>
            
            <q-card-section>
              <div class="text-subtitle2">Category Scores</div>
              <div v-for="(score, category) in currentScore.categoryScores" :key="category" class="q-mt-sm">
                <div class="row items-center">
                  <div class="col">{{ category }}</div>
                  <div class="col-auto">
                    <q-linear-progress
                      :value="score / 10"
                      :color="getScoreColor(score)"
                      class="q-ml-md"
                      style="width: 100px"
                    />
                    <span class="q-ml-sm">{{ score.toFixed(1) }}</span>
                  </div>
                </div>
              </div>
            </q-card-section>
          </q-card>
        </div>
      </div>
    </div>

    <!-- POI List Panel -->
    <div class="col-12 col-md-4">
      <q-card class="poi-panel">
        <q-card-section>
          <div class="text-h6">Nearby Points of Interest</div>
        </q-card-section>

        <q-card-section>
          <q-list>
            <q-item v-for="poi in nearbyPois" :key="poi.id" clickable @click="selectPoi(poi)">
              <q-item-section avatar>
                <q-icon :name="getPoiIcon(poi.category)" :color="getPoiColor(poi.category)" />
              </q-item-section>
              
              <q-item-section>
                <q-item-label>{{ poi.name }}</q-item-label>
                <q-item-label caption>
                  {{ poi.category }} • {{ poi.distanceKm.toFixed(1) }}km
                </q-item-label>
              </q-item-section>

              <q-item-section side>
                <q-chip :color="getScoreColor(poi.score)" text-color="white" dense>
                  {{ poi.score.toFixed(1) }}
                </q-chip>
              </q-item-section>
            </q-item>
          </q-list>
        </q-card-section>
      </q-card>
    </div>

    <!-- Loading Overlay -->
    <q-inner-loading :showing="isLoading">
      <q-spinner-dots size="50px" color="primary" />
    </q-inner-loading>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'
import { servicesConfig } from 'src/config'

// Reactive data
const mapContainer = ref<HTMLElement>()
const searchAddress = ref('')
const transportationMode = ref<'car' | 'walking'>('car')
const isLoading = ref(false)
const isSearching = ref(false)
const currentScore = ref<{
  overallScore: number
  categoryScores: Record<string, number>
  poiScores: Array<{
    poiId: string
    category: string
    name: string
    distanceKm: number
    score: number
    weight: number
  }>
  transportationMode: string
  location: string
} | null>(null)
const nearbyPois = ref<Array<{
  id: string
  name: string
  category: string
  latitude: number
  longitude: number
  address: string
  description: string
  distanceKm: number
  score: number
}>>([])

// Map instance
let map: L.Map | null = null
let markers: L.Marker[] = []
let selectedMarker: L.Marker | null = null

// Lombardy center coordinates
const LOMBARDY_CENTER: [number, number] = [45.6983, 9.6773]

onMounted(() => {
  initializeMap()
})

onUnmounted(() => {
  if (map) {
    map.remove()
  }
})

function initializeMap() {
  if (!mapContainer.value) return

  // Initialize the map
  map = L.map(mapContainer.value).setView(LOMBARDY_CENTER, 8)

  // Add OpenStreetMap tiles
  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '© OpenStreetMap contributors'
  }).addTo(map)

  // Add click handler
  map.on('click', (e) => {
    const { lat, lng } = e.latlng
    void searchLocationByCoordinates(lat, lng)
  })
}

async function searchLocation() {
  if (!searchAddress.value.trim()) return

  isSearching.value = true
  try {
    // Call geocoding service
    const response = await fetch(`${servicesConfig.geocodingService}/api/geocoding/geocode?address=${encodeURIComponent(searchAddress.value)}`)
    const data = await response.json()

    if (data.success) {
      const { latitude, longitude } = data
      void searchLocationByCoordinates(latitude, longitude)
    } else {
      // Show error
      console.error('Geocoding failed:', data.message)
    }
  } catch (error) {
    console.error('Error searching location:', error)
  } finally {
    isSearching.value = false
  }
}

async function searchLocationByCoordinates(lat: number, lng: number) {
  isLoading.value = true
  
  try {
    // Clear existing markers
    clearMarkers()

    // Add selected location marker
    selectedMarker = L.marker([lat, lng], {
      icon: L.divIcon({
        className: 'selected-marker',
        html: '<div style="background-color: #1976d2; width: 20px; height: 20px; border-radius: 50%; border: 3px solid white;"></div>',
        iconSize: [20, 20] as [number, number]
      })
    }).addTo(map!)

    // Center map on selected location
    map!.setView([lat, lng], 14)

    // Get POIs
    const poiResponse = await fetch(`${servicesConfig.poiService}/api/poi/search`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        latitude: lat,
        longitude: lng,
        radiusKm: 5.0,
        maxResults: 50
      })
    })

    const poiData = await poiResponse.json()
    nearbyPois.value = poiData.pois

    // Add POI markers
    poiData.pois.forEach((poi: {
      id: string
      name: string
      category: string
      latitude: number
      longitude: number
      address: string
      description: string
      distanceKm: number
    }) => {
      const marker = L.marker([poi.latitude, poi.longitude], {
        icon: L.divIcon({
          className: 'poi-marker',
          html: `<div style="background-color: ${getPoiColor(poi.category)}; width: 15px; height: 15px; border-radius: 50%; border: 2px solid white;"></div>`,
          iconSize: [15, 15] as [number, number]
        })
      }).addTo(map!)

      marker.bindPopup(`
        <div>
          <strong>${poi.name}</strong><br>
          ${poi.category}<br>
          ${poi.address}
        </div>
      `)

      markers.push(marker)
    })

    // Calculate score
    const scoreResponse = await fetch(`${servicesConfig.scoringService}/api/scoring/calculate`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        latitude: lat,
        longitude: lng,
        transportationMode: transportationMode.value,
        poiDistances: poiData.pois.map((poi: {
          id: string
          category: string
          distanceKm: number
          name: string
        }) => ({
          poiId: poi.id,
          category: poi.category,
          distanceKm: poi.distanceKm,
          name: poi.name
        }))
      })
    })

    const scoreData = await scoreResponse.json()
    currentScore.value = scoreData

  } catch (error) {
    console.error('Error processing location:', error)
  } finally {
    isLoading.value = false
  }
}

function clearMarkers() {
  markers.forEach(marker => marker.remove())
  markers = []
  
  if (selectedMarker) {
    selectedMarker.remove()
    selectedMarker = null
  }
}

function selectPoi(poi: {
  id: string
  name: string
  category: string
  latitude: number
  longitude: number
  address: string
  description: string
  distanceKm: number
  score: number
}) {
  if (map) {
    map.setView([poi.latitude, poi.longitude], 16)
  }
}

function getPoiIcon(category: string): string {
  const icons: Record<string, string> = {
    schools: 'school',
    hospitals: 'local_hospital',
    supermarkets: 'shopping_cart',
    pharmacies: 'local_pharmacy',
    banks: 'account_balance',
    post_offices: 'mail',
    police_stations: 'security',
    fire_stations: 'fire_truck',
    libraries: 'local_library',
    museums: 'museum',
    parks: 'park',
    restaurants: 'restaurant',
    shopping_centers: 'shopping_bag'
  }
  return icons[category] || 'place'
}

function getPoiColor(category: string): string {
  const colors: Record<string, string> = {
    schools: '#4CAF50',
    hospitals: '#F44336',
    supermarkets: '#FF9800',
    pharmacies: '#9C27B0',
    banks: '#2196F3',
    post_offices: '#607D8B',
    police_stations: '#3F51B5',
    fire_stations: '#FF5722',
    libraries: '#795548',
    museums: '#E91E63',
    parks: '#8BC34A',
    restaurants: '#FFC107',
    shopping_centers: '#00BCD4'
  }
  return colors[category] || '#757575'
}

function getScoreColor(score: number): string {
  if (score >= 8) return 'positive'
  if (score >= 6) return 'warning'
  return 'negative'
}
</script>

<style scoped>
.map-container {
  position: relative;
  height: 100vh;
}

#map {
  height: 100%;
  width: 100%;
}

.search-panel {
  position: absolute;
  top: 20px;
  left: 20px;
  z-index: 1000;
  max-width: 300px;
}

.search-card {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
}

.score-panel {
  position: absolute;
  top: 20px;
  right: 20px;
  z-index: 1000;
  max-width: 300px;
}

.score-card {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
}

.poi-panel {
  height: 100vh;
  overflow-y: auto;
}

:deep(.selected-marker) {
  background: transparent;
  border: none;
}

:deep(.poi-marker) {
  background: transparent;
  border: none;
}
</style>
