import PagingRequest from "../common/PagingRequest";
import { EActivityType } from "../enums/EActivityType";

export default interface UserRoleSubscriptionActivityPagingRequest extends PagingRequest {
    activityType: EActivityType;
} 