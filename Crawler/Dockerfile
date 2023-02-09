FROM node:16

RUN apt-get update && apt-get install -yq libgconf-2-4

# Install latest chrome dev package and fonts to support major charsets (Chinese, Japanese, Arabic, Hebrew, Thai and a few others)
# Note: this installs the necessary libs to make the bundled version of Chromium that Puppeteer
# installs, work.
RUN apt-get update && apt-get install -y wget --no-install-recommends --allow-unauthenticated \
  && wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - \
  && sh -c 'echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google.list' \
  && apt-get update \
  && apt-get install -y --no-install-recommends --allow-unauthenticated \
  google-chrome-unstable fonts-ipafont-gothic fonts-wqy-zenhei fonts-thai-tlwg fonts-kacst ttf-freefont \
  && rm -rf /var/lib/apt/lists/* \
  && apt-get purge --auto-remove -y curl \
  && rm -rf /src/*.deb

RUN groupadd -r crawler && useradd -r -g crawler -G audio,video crawler \
  && mkdir -p /home/crawler/app \
  && chown -R crawler:crawler /home/crawler

# Run everything after as non-privileged user.
USER crawler

WORKDIR /home/crawler/app
COPY *.json ./

RUN npm ci --production
COPY src ./src
COPY scripts ./scripts

EXPOSE ${INDEXER_TEST_PORT:-4957}

CMD [ "npm", "start" ]