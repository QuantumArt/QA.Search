#!/bin/sh
docker pull '||registry-source||/||integration-image-source||:||version||' && \
docker tag '||registry-source||/||integration-image-source||:||version||' '||registry-destination||/||integration-image-destination||:||version||' && \
docker push '||registry-destination||/||integration-image-destination||:||version||'