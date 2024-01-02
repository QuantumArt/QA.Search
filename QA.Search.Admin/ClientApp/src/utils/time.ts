const offset: number = new Date().getTimezoneOffset();

export default function dateConvertToLocal(dateString: string | null | undefined): string {
  if (!dateString) {
    return "";
  } 

  let date: Date = parseStringToDate(dateString);
  date.setMinutes(date.getMinutes() - offset);

  return date.toLocaleString();
}

function parseStringToDate(dateString: string): Date {
  let dateAndTime: string[] = dateString.split(" ");
  let date: string[] = dateAndTime[0].split(".");
  let time = dateAndTime[1].split(":");

  return new Date(
    parseInt("20" + date[2]),
    parseInt(date[1], 10) - 1,
    parseInt(date[0]),
    parseInt(time[0]),
    parseInt(time[1]),
    parseInt(time[2])
  );
}
