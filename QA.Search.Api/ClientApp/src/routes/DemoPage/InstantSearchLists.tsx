import React from "react";
import { Divider, Icon } from "@blueprintjs/core";
import { ElasticDocument } from "../../SearchClient";
import { News, Help, MediaMaterial, TextPage, MediaPage, MobileApp } from "./models";

export function renderMobileApps(foundMobileApps: MobileApp[]) {
  return (
    <>
      <div className="instant-search__header">Мобильные приложения</div>
      {foundMobileApps.map(app => {
        const html = getMobileAppHtml(app);
        return (
          <a key={app._id} className="instant-search__card" href={app.SearchUrl}>
            <div className="instant-search__short-card">
              {renderMobileAppImage(app)}
              <div className="instant-search__card-text" dangerouslySetInnerHTML={html} />
            </div>
            <div className="instant-search__full-card">
              {renderMobileAppImage(app)}
              <div className="instant-search__card-text" dangerouslySetInnerHTML={html} />
            </div>
          </a>
        );
      })}
      <Divider />
    </>
  );
}

function renderMobileAppImage(app: MobileApp) {
  return app.Icon ? (
    <img
      className="instant-search__card-image instant-search__app-image"
      src={"http://msk.alpha.domain.ru/upload/contents/10695/" + app.Icon}
      alt="preview"
      width="48"
      height="48"
    />
  ) : (
    <Icon icon="mobile-phone" className="instant-search__card-image" iconSize={32} />
  );
}

export function renderMedia(foundMaterials: MediaMaterial[], foundMediaPages: MediaPage[]) {
  return (
    <>
      <div className="instant-search__header">Медиа</div>
      {foundMaterials.map(material => {
        const html = getSnippetsHtml(material);
        return (
          <a key={material._id} className="instant-search__card" href={material.SearchUrl}>
            <div className="instant-search__short-card">
              {renderMaterialImage(material)}
              <div className="instant-search__card-text" dangerouslySetInnerHTML={html} />
            </div>
            <div className="instant-search__full-card">
              {renderMaterialImage(material)}
              <div className="instant-search__card-text" dangerouslySetInnerHTML={html} />
            </div>
          </a>
        );
      })}
      {foundMediaPages.map(page => {
        const html = getSnippetsHtml(page);
        return (
          <a key={page._id} className="instant-search__card" href={page.SearchUrl}>
            <div className="instant-search__short-card">
              <Icon icon="media" className="instant-search__card-image" iconSize={32} />
              <div className="instant-search__card-text" dangerouslySetInnerHTML={html} />
            </div>
            <div className="instant-search__full-card">
              <Icon icon="media" className="instant-search__card-image" iconSize={32} />
              <div className="instant-search__card-text" dangerouslySetInnerHTML={html} />
            </div>
          </a>
        );
      })}
      <Divider />
    </>
  );
}

export function renderMaterialImage(material: MediaMaterial) {
  return material.Preview ? (
    <img
      className="instant-search__card-image instant-search__material-image"
      src={"https://static.ssl.domain.ru/media/images/materials/" + material.Preview}
      alt="preview"
      width="48"
      height="48"
    />
  ) : (
    <Icon icon="media" className="instant-search__card-image" iconSize={32} />
  );
}

export function renderPages(foundPages: TextPage[]) {
  return (
    <>
      <div className="instant-search__header">Частным клиентам</div>
      {foundPages.map(page => {
        const html = getSnippetsHtml(page);
        return (
          <a key={page._id} className="instant-search__card" href={page.SearchUrl}>
            <div className="instant-search__short-card">
              <Icon icon="book" className="instant-search__card-image" iconSize={32} />
              <div className="instant-search__card-text" dangerouslySetInnerHTML={html} />
            </div>
            <div className="instant-search__full-card">
              <Icon icon="book" className="instant-search__card-image" iconSize={32} />
              <div className="instant-search__card-text" dangerouslySetInnerHTML={html} />
            </div>
          </a>
        );
      })}
      <Divider />
    </>
  );
}

export function renderNews(foundNews: News[]) {
  return (
    <>
      <div className="instant-search__header">Новости</div>
      {foundNews.map(news => {
        const html = getNewsHtml(news);
        return (
          <a key={news._id} className="instant-search__card" href={news.SearchUrl}>
            <div className="instant-search__short-card">
              <Icon icon="star" className="instant-search__card-image" iconSize={32} />
              <div className="instant-search__card-text" dangerouslySetInnerHTML={html} />
            </div>
            <div className="instant-search__full-card">
              <Icon icon="star" className="instant-search__card-image" iconSize={32} />
              <div className="instant-search__card-text" dangerouslySetInnerHTML={html} />
            </div>
          </a>
        );
      })}
      <Divider />
    </>
  );
}

export function renderHelp(foundHelp: Help[]) {
  return (
    <>
      <div className="instant-search__header">Помощь</div>
      {foundHelp.map(help => {
        const html = getSnippetsHtml(help);
        return (
          <a key={help._id} className="instant-search__card" href={help.SearchUrl}>
            <div className="instant-search__short-card">
              <Icon icon="help" className="instant-search__card-image" iconSize={32} />
              <div className="instant-search__card-text" dangerouslySetInnerHTML={html} />
            </div>
            <div className="instant-search__full-card">
              <Icon icon="help" className="instant-search__card-image" iconSize={32} />
              <div className="instant-search__card-text" dangerouslySetInnerHTML={html} />
            </div>
          </a>
        );
      })}
      <Divider />
    </>
  );
}

function getSnippetsHtml(document: ElasticDocument) {
  const snippets = Object.values(document._snippets!)
    .flat()
    .map(snippet => snippet.trim())
    .filter(Boolean)
    .join(" | ");

  return { __html: snippets };
}

function getMobileAppHtml(app: MobileApp) {
  const { __html: snippets } = getSnippetsHtml(app);

  const text = `[${app.Title}] ${snippets}`;

  return { __html: text };
}

function getNewsHtml(news: News) {
  const { __html: snippets } = getSnippetsHtml(news);

  const text = `[${news.PublishDate.toLocaleDateString()}] ${snippets}`;

  return { __html: text };
}
