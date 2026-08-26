<template>
    <div class="weather-component">
        <h1>Weather forecast</h1>
        <p>This component demonstrates fetching data from the server.</p>

        <div v-if="isFetching" class="loading">
            Loading... Please refresh once the ASP.NET backend has started. See <a
                href="https://aka.ms/jspsintegrationvue">https://aka.ms/jspsintegrationvue</a> for more details.
        </div>

        <div>
            <button @click="() => refetch()" role="button">
                Refresh
            </button>
        </div>

        <div v-if="data" class="content">
            <table>
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Temp. (C)</th>
                        <th>Temp. (F)</th>
                        <th>Summary</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="forecast in data" :key="forecast.date?.year">
                        <td>{{ forecast.date }}</td>
                        <td>{{ forecast.temperatureC?.value }}</td>
                        <td>{{ forecast.temperatureF?.value }}</td>
                        <td>{{ forecast.summary }}</td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
</template>

<!--<script lang="js">
    import { defineComponent } from 'vue';

    export default defineComponent({
        data() {
            return {
                loading: false,
                post: null
            };
        },
        async created() {
            // fetch the data when the view is created and the data is
            // already being observed
            await this.fetchData();
        },
        watch: {
            // call again the method if the route changes
            '$route': 'fetchData'
        },
        methods: {
            async fetchData() {
                this.post = null;
                this.loading = true;

            var response = await fetch('api/weatherforecast');
                if (response.ok) {
                    this.post = await response.json();
                    this.loading = false;
                }
            }
        },
    });
</script>-->

<script setup lang="ts">
import { AnonymousAuthenticationProvider } from "@microsoft/kiota-abstractions";
import { FetchRequestAdapter } from "@microsoft/kiota-http-fetchlibrary";
import { createDishesClient } from "@/client/dishesClient.js"
import { WeatherForecast } from "@/client/models/index.js";

import { useQuery } from '@tanstack/vue-query'

// API requires no authentication, so use the anonymous
// authentication provider
const authProvider = new AnonymousAuthenticationProvider();
// Create request adapter using the fetch-based implementation
const adapter = new FetchRequestAdapter(authProvider);
// Create the API client
const client = createDishesClient(adapter);

// const fetchWeatherForecast = async () => {
//     const response = await fetch('api/weatherforecast');
//     if (!response.ok) {
//         throw new Error('Network response was not ok');
//     }
//     return response.json();
// };

const fetchWeatherForecast = async () => await client.api.weatherforecast.get();

const { isFetching, data, refetch } = useQuery({
    queryKey: ['weatherforecast'],
    queryFn: fetchWeatherForecast,
})
</script>

<style scoped>
th {
    font-weight: bold;
}

th,
td {
    padding-left: .5rem;
    padding-right: .5rem;
}

.weather-component {
    text-align: center;
}

table {
    margin-left: auto;
    margin-right: auto;
}

button {
    appearance: none;
    background-color: #2ea44f;
    border: 1px solid rgba(27, 31, 35, .15);
    border-radius: 6px;
    box-shadow: rgba(27, 31, 35, .1) 0 1px 0;
    box-sizing: border-box;
    color: #fff;
    cursor: pointer;
    display: inline-block;
    font-family: -apple-system, system-ui, "Segoe UI", Helvetica, Arial, sans-serif, "Apple Color Emoji", "Segoe UI Emoji";
    font-size: 14px;
    font-weight: 600;
    line-height: 20px;
    padding: 6px 16px;
    position: relative;
    text-align: center;
    text-decoration: none;
    user-select: none;
    -webkit-user-select: none;
    touch-action: manipulation;
    vertical-align: middle;
    white-space: nowrap;

    :focus:not(:focus-visible):not(.focus-visible) {
        box-shadow: none;
        outline: none;
    }

    :hover {
        background-color: #2c974b;
    }

    :focus {
        box-shadow: rgba(46, 164, 79, .4) 0 0 0 3px;
        outline: none;
    }

    :disabled {
        background-color: #94d3a2;
        border-color: rgba(27, 31, 35, .1);
        color: rgba(255, 255, 255, .8);
        cursor: default;
    }

    :active {
        background-color: #298e46;
        box-shadow: rgba(20, 70, 32, .2) 0 1px 0 inset;
    }
}
</style>
