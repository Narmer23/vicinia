<template>
  <q-layout view="lHh Lpr lFf">
    <q-header elevated class="bg-primary text-white">
      <q-toolbar>
        <q-btn
          flat
          dense
          round
          icon="menu"
          aria-label="Menu"
          @click="toggleLeftDrawer"
        />

        <q-toolbar-title>
          <q-avatar>
            <img src="~assets/quasar-logo-vertical.svg" />
          </q-avatar>
          Vicinia
        </q-toolbar-title>

        <div class="row items-center q-gutter-sm">
          <q-btn-dropdown flat color="white" icon="language" label="EN">
            <q-list>
              <q-item clickable v-close-popup @click="setLocale('en')">
                <q-item-section>
                  <q-item-label>English</q-item-label>
                </q-item-section>
              </q-item>
              <q-item clickable v-close-popup @click="setLocale('it')">
                <q-item-section>
                  <q-item-label>Italiano</q-item-label>
                </q-item-section>
              </q-item>
            </q-list>
          </q-btn-dropdown>

          <q-btn flat round icon="account_circle" v-if="!isAuthenticated" @click="showLoginDialog = true" />
          <q-btn flat round icon="logout" v-else @click="logout" />
        </div>
      </q-toolbar>
    </q-header>

    <q-drawer
      v-model="leftDrawerOpen"
      show-if-above
      bordered
      class="bg-grey-1"
    >
      <q-list>
        <q-item-label header>
          Navigation
        </q-item-label>

        <EssentialLink
          v-for="link in essentialLinks"
          :key="link.title"
          v-bind="link"
        />

        <q-separator />

        <q-item-label header>
          Account
        </q-item-label>

        <q-item clickable v-if="!isAuthenticated" @click="showLoginDialog = true">
          <q-item-section avatar>
            <q-icon name="login" />
          </q-item-section>
          <q-item-section>Login</q-item-section>
        </q-item>

        <q-item clickable v-if="!isAuthenticated" @click="showRegisterDialog = true">
          <q-item-section avatar>
            <q-icon name="person_add" />
          </q-item-section>
          <q-item-section>Register</q-item-section>
        </q-item>

        <q-item clickable v-if="isAuthenticated" @click="showHistory = true">
          <q-item-section avatar>
            <q-icon name="history" />
          </q-item-section>
          <q-item-section>Search History</q-item-section>
        </q-item>

        <q-item clickable v-if="isAuthenticated" @click="logout">
          <q-item-section avatar>
            <q-icon name="logout" />
          </q-item-section>
          <q-item-section>Logout</q-item-section>
        </q-item>
      </q-list>
    </q-drawer>

    <q-page-container>
      <router-view />
    </q-page-container>

    <!-- Login Dialog -->
    <q-dialog v-model="showLoginDialog">
      <q-card style="min-width: 350px">
        <q-card-section>
          <div class="text-h6">Login</div>
        </q-card-section>

        <q-card-section class="q-pt-none">
          <q-input
            v-model="loginForm.email"
            label="Email"
            type="email"
            outlined
            dense
          />
          <q-input
            v-model="loginForm.password"
            label="Password"
            type="password"
            outlined
            dense
            class="q-mt-md"
          />
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat label="Cancel" color="primary" v-close-popup />
          <q-btn label="Login" color="primary" @click="login" />
        </q-card-actions>
      </q-card>
    </q-dialog>

    <!-- Register Dialog -->
    <q-dialog v-model="showRegisterDialog">
      <q-card style="min-width: 350px">
        <q-card-section>
          <div class="text-h6">Register</div>
        </q-card-section>

        <q-card-section class="q-pt-none">
          <q-input
            v-model="registerForm.email"
            label="Email"
            type="email"
            outlined
            dense
          />
          <q-input
            v-model="registerForm.password"
            label="Password"
            type="password"
            outlined
            dense
            class="q-mt-md"
          />
          <q-input
            v-model="registerForm.confirmPassword"
            label="Confirm Password"
            type="password"
            outlined
            dense
            class="q-mt-md"
          />
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat label="Cancel" color="primary" v-close-popup />
          <q-btn label="Register" color="primary" @click="register" />
        </q-card-actions>
      </q-card>
    </q-dialog>

    <!-- History Dialog -->
    <q-dialog v-model="showHistory" maximized>
      <q-card>
        <q-card-section class="row items-center q-pb-none">
          <div class="text-h6">Search History</div>
          <q-space />
          <q-btn icon="close" flat round dense v-close-popup />
        </q-card-section>

        <q-card-section>
          <q-list>
            <q-item v-for="item in searchHistory" :key="item.id" clickable @click="loadHistoryItem(item)">
              <q-item-section>
                <q-item-label>{{ item.address }}</q-item-label>
                <q-item-label caption>
                  Score: {{ item.overallScore }} | Mode: {{ item.transportationMode }}
                </q-item-label>
              </q-item-section>
              <q-item-section side>
                {{ formatDate(item.searchDate) }}
              </q-item-section>
            </q-item>
          </q-list>
        </q-card-section>
      </q-card>
    </q-dialog>
  </q-layout>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import EssentialLink from 'components/EssentialLink.vue'

const leftDrawerOpen = ref(false)
const showLoginDialog = ref(false)
const showRegisterDialog = ref(false)
const showHistory = ref(false)

const isAuthenticated = ref(false)
const searchHistory = ref<Array<{
  id: string
  address: string
  overallScore: number
  transportationMode: string
  searchDate: string
}>>([])

const loginForm = ref({
  email: '',
  password: ''
})

const registerForm = ref({
  email: '',
  password: '',
  confirmPassword: ''
})

const essentialLinks = [
  {
    title: 'Home',
    caption: 'Search for locations',
    icon: 'home',
    link: '/'
  },
  {
    title: 'About',
    caption: 'About Vicinia',
    icon: 'info',
    link: '/about'
  }
]

function toggleLeftDrawer() {
  leftDrawerOpen.value = !leftDrawerOpen.value
}

function setLocale(lang: string) {
  console.log('Set locale:', lang)
}

function login() {
  // TODO: Implement login logic
  console.log('Login:', loginForm.value)
  showLoginDialog.value = false
}

function register() {
  // TODO: Implement register logic
  console.log('Register:', registerForm.value)
  showRegisterDialog.value = false
}

function logout() {
  // TODO: Implement logout logic
  isAuthenticated.value = false
}

function loadHistoryItem(item: {
  id: string
  address: string
  overallScore: number
  transportationMode: string
  searchDate: string
}) {
  // TODO: Implement history loading
  console.log('Load history item:', item)
  showHistory.value = false
}

function formatDate(date: string) {
  return new Date(date).toLocaleDateString()
}
</script>
