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

interface SiteRegion {
  Alias: string;
}

interface NewsGroup {
  Title: string;
}

export interface QueryCompletion extends ElasticDocument {
  key: string;
}