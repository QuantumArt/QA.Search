#!/bin/sh
docker pull '||registry-source||/||api-image-source||:||version||' && \
docker tag '||registry-source||/||api-image-source||:||version||' '||registry-destination||/||api-image-destination||:||version||' && \
docker push '||registry-destination||/||api-image-destination||:||version||'