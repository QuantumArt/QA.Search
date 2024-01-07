import dateAndTime from "date-and-time";
import twoDigitYear from "date-and-time/plugin/two-digit-year";

export default function dateConvertToLocal(dateString: string | null | undefined): string {
  if (!dateString) {
    return "";
  }

  dateAndTime.plugin(twoDigitYear);
  const date = dateAndTime.parse(dateString, "DD.MM.YY HH:mm:ss", true);

  return date.toLocaleString();
}
