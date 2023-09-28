#!/bin/sh
docker pull '||registry-source||/||admin-image-source||:||version||' && \
docker tag '||registry-source||/||admin-image-source||:||version||' '||registry-destination||/||admin-image-destination||:||version||' && \
docker push '||registry-destination||/||admin-image-destination||:||version||'