import { ElasticDocument } from "../../SearchClient";

export interface SearchItem extends ElasticDocument {
  SearchArea?: string;
  SearchUrl: string;
  Title?: string;
  HeaderTitle?: string;
  MarketingProduct?: { Title?: string };
  Regions?: SiteRegion[];
  Groups?: NewsGroup[];
  HeaderLead?: string;
  PublishDate?: Date;
  Icon?: string;
  Preview?: string;
}

export interface MobileApp extends ElasticDocument {
  SearchArea: string;
  SearchUrl: string;
  Icon: string;
  Description: string;
  ShortDescription: string;
  Title: string;
}

export interface News extends ElasticDocument {
  SearchArea: string;
  SearchUrl: string;
  PublishDate: Date;
  Title: string;
  Anounce: string;
  Text: string;
  Regions: SiteRegion[];
  Groups: NewsGroup[];
}

export interface TextPage extends ElasticDocument {
  SearchArea: string;
  SearchUrl: string;
  Title: string;
  Description: string;
  MetaDescription: string;
  Keywords: string;
  Text: string;
  Regions: SiteRegion[];
}

export interface Help extends ElasticDocument {
  SearchArea: string;
  SearchUrl: string;
  Title: string;
  Description: string;
  Regions: SiteRegion[];
}

export interface MediaMaterial extends ElasticDocument {
  SearchUrl: string;
  Preview: string;
}

export interface MediaPage extends ElasticDocument {
  SearchUrl: string;
  Title: string;
  TitleOnPage: string;
  Description: string;
}

interface SiteRegion {
  Alias: string;
}

interface NewsGroup {
  Title: string;
}
