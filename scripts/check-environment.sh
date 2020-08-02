#!/bin/bash

if [ -z "$WEATHERSTDR_WUND_API_KEY" ]; then
    echo -e "\e[1;31mERROR - 'WEATHERSTDR_WUND_API_KEY' has not been provided! \e[0m"
    exit 1
fi

# If it has been provided, we should use sasl for kafka connection
if ! [[ -z "${WEATHERSTDR_KERBEROS_PUBLIC_URL}" ]]; then
    echo "INFO - 'WEATHERSTDR_KERBEROS_PUBLIC_URL' environment variable has been provided. Testing required environment variables for Kafka Kerberos SASL authentication"

    if [[ -z "${WEATHERSTDR_KERBEROS_REALM}" ]]; then
        echo -e "\e[1;31mERROR - Missing 'WEATHERSTDR_KERBEROS_REALM' environment variable. This is required to enable Kafka Kerberos SASL authentication \e[0m"
        exit 1
    fi

    # If they haven't provided their own keytab in volumes, it is tested if they have provided the necessary environment variables to download the keytab from an API
    if [[ -z "${WEATHERSTDR_KERBEROS_PRINCIPAL}" ]]; then
        if [[ -z "${WEATHERSTDR_KERBEROS_API_URL}" ]]; then
            echo -e "\e[1;31mERROR - One of either 'WEATHERSTDR_KERBEROS_PRINCIPAL' or 'WEATHERSTDR_KERBEROS_API_URL' must be supplied! It is required to enable Kafka Kerberos SASL authentication \e[0m"
            exit 1
        else # the user wants to use cfei kerberos api to get keytabs

            if [[ -z "${WEATHERSTDR_KERBEROS_API_SERVICE_USERNAME}" ]]; then
                echo -e "\e[1;31mERROR - Missing 'WEATHERSTDR_KERBEROS_API_SERVICE_USERNAME' environment variable. This is required to enable Kafka Kerberos SASL authentication \e[0m"
                exit 1
            fi

            if [[ -z "${WEATHERSTDR_KERBEROS_API_SERVICE_PASSWORD}" ]]; then
                echo -e "\e[1;31mERROR - Missing 'WEATHERSTDR_KERBEROS_API_SERVICE_PASSWORD' environment variable. This is required to enable Kafka Kerberos SASL authentication \e[0m"
                exit 1
            fi

            # response will be 'FAIL' if it can't connect or if the url returned an error
            response=$(curl --fail --connect-timeout 5 --retry 5 --retry-delay 5 --retry-max-time 30 --retry-connrefused --max-time 5 -X POST -H "Content-Type: application/json" -d "{\"username\":\""$WEATHERSTDR_KERBEROS_API_SERVICE_USERNAME"\", \"password\":\""$WEATHERSTDR_KERBEROS_API_SERVICE_PASSWORD"\"}" "$WEATHERSTDR_KERBEROS_API_URL" -o "$KEYTAB_LOCATION" && echo "INFO - Using the keytab from the API and a principal name of '"$WEATHERSTDR_KERBEROS_API_SERVICE_USERNAME"'@'"$WEATHERSTDR_KERBEROS_REALM"'" || echo "FAIL")
            if [ "$response" == "FAIL" ]; then
                echo -e "\e[1;31mERROR - Kerberos API did not succeed when fetching keytab. See curl error above for further details \e[0m"
                exit 1
            else
                echo "INFO - Successfully communicated with kerberos and logged in"
            fi
        fi

    else # The user has supplied their own principals
        # test if a keytab has been provided and if it's in the expected directory
        if ! [[ -f "${KEYTAB_LOCATION}" ]]; then
            echo -e "\e[1;31mERROR - Missing kerberos keytab file '"$KEYTAB_LOCATION"'. This is required to enable kerberos when 'WEATHERSTDR_KERBEROS_PRINCIPAL' is supplied. Provide it with a docker volume or docker mount \e[0m"
            exit 1
        else
            echo "INFO - Using the supplied keytab and the principal from environment variable 'WEATHERSTDR_KERBEROS_PRINCIPAL' "
        fi
    fi

    if [ -z "$WEATHERSTDR_BROKER_KERBEROS_SERVICE_NAME" ]; then
        echo "INFO - 'WEATHERSTDR_BROKER_KERBEROS_SERVICE_NAME' has not been provided. Defaulting to 'kafka' "
    fi

    # configuring krb5.conf files so acl-manager can communicate with the kerberos server and ensure the provided keytab is correct
    configure-krb5.sh "$WEATHERSTDR_KERBEROS_REALM" "$WEATHERSTDR_KERBEROS_PUBLIC_URL"

else
    echo "INFO - Missing 'WEATHERSTDR_KERBEROS_PUBLIC_URL' environment variable. Will not enable Kafka Kerberos SASL authentication"
fi
