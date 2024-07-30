import { EActivityType } from "../enums/EActivityType";

export default interface ActivityLogResponse {
    id: number;
    description?: string | null;
    ipv4Address?: string | null;
    ipv6Address?: string | null;
    activityType: EActivityType;
    logTimes: number;
    userId: number;
    createdOnUtc: Date;
}