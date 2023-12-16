const offset: number = new Date().getTimezoneOffset();

export default function dateConvertToLocal(dateString: string | null | undefined): string {
  if (dateString === "" || dateString === null || dateString === undefined) {
    return "";
  }

  let date: Date = parceStringToDate(dateString);
  date.setMinutes(date.getMinutes() - offset);

  return date.toLocaleString();
}

function parceStringToDate(dateString: string): Date {
  let dataAndTime: string[] = dateString.split(" ");
  let date: string[] = dataAndTime[0].split(".");
  let time = dataAndTime[1].split(":");

  return new Date(
    parseInt("20" + date[2]),
    parseInt(date[1], 10) - 1,
    parseInt(date[0]),
    parseInt(time[0]),
    parseInt(time[1]),
    parseInt(time[2])
  );
}
