# Wunderground-Docker

You can find station IDs here:
https://www.wunderground.com/wundermap 

# How to use
This docker-compose file shows the deployment of the dashboard-server container.

```
version: "3"

services:
  wunderground:
    image: cfei/weather-station-driver
    container_name: weather_station_driver
    environment:
      WEATHERSTDR_WUND_API_KEY: API_KEY
```

# Configuration
#### Required environment variables

- `WEATHERSTDR_WUND_API_KEY`: API key used for communication with the wunderground API.

#### Optional environment variables

- `WEATHERSTDR_KAFKA_URL`: Comma seperated list of one or more kafka urls. It will default to cfei's own kafka cluster 'kafka1.cfei.dk:9092,kafka2.cfei.dk:9092,kafka3.cfei.dk:9092'.

- `WEATHERSTDR_INITIAL_SUBSCRIPTIONS`: With this variable it's possible to define wunderground stations which the driver will start to pull from, once started. It has to be a comma-seperated string with the format "StationId=REQUIRED;Interval=REQUIRED". Where the interval is in seconds. (example: `StationId=IODENS3;Interval=5,StationId=IKASTR4;Interval=10`). Please note that if you have specified this environment variable and the container restarts at some point, the subscriptions will not be added again. This environment variable is only effective the very first time the container is started.

- `WEATHERSTDR_KERBEROS_PUBLIC_URL`: Public URL of the kerberos server to use. Required if the Kafka URL has SASL authentication.

- `WEATHERSTDR_KERBEROS_REALM`: The realm to use on the kerberos server. Required if the Kafka URL has SASL authentication.

- `WEATHERSTDR_KERBEROS_API_URL`: The URL to use when the driver fetches its keytab from the kerberos server. The URL has to point to an HTTP POST Endpoint. The image will then supply the values of 'WEATHERSTDR_KERBEROS_API_SERVICE_USERNAME' and 'WEATHERSTDR_KERBEROS_API_SERVICE_PASSWORD' to the request.

- `WEATHERSTDR_KERBEROS_API_SERVICE_USERNAME`: The username to use when fetching keytab on 'WEATHERSTDR_KERBEROS_API_URL'.

- `WEATHERSTDR_KERBEROS_API_SERVICE_PASSWORD`: The password to use when fetching keytab on 'WEATHERSTDR_KERBEROS_API_URL'.

- `WEATHERSTDR_BROKER_KERBEROS_SERVICE_NAME`: This should be the principal name of the service keytab the kafka broker(s) uses. So if kafka uses a keytab with a principal name of 'kafka'. This environment variable should be 'kafka'. It will default to 'kafka' if nothing is provided and kerberos is activated otherwise.

- `WEATHERSTDR_KERBEROS_PRINCIPAL`: The principal that the driver should use from the kerberos server. Required if you want to supply your own keytab through volumes.

- `WEATHERSTDR_SUBSCRIPTION_ACTION_TOPIC`: If you would like to manually configure what topic the driver listens for subscription actions through kafka, you can se it with this variable. Default is the topic 'subscriptions-actions'.

- `WEATHERSTDR_NEW_SUBSCRIPTION_RESPONSE_TOPIC`: If you send a request to start a new subscription and you would like a confirmation / error on that request you can listen on this topic. There will be responses with a HTTP status code and a message. It defaults to 'new-subscriptions-response'
  
- `WEATHERSTDR_WEATHER_DATA_TOPIC`: The kafka-topic the driver will send data to. Default is the topic 'weather-data'

## Sending a Subscription action
TODO