import { Link, useParams } from "react-router-dom";
import Pagination from "../../components/shared/Pagination";
import { useTranslation } from "react-i18next";
import { useSelector } from "react-redux";
import { StoreState, useAppDispatch } from "../../store";
import React, { useEffect, useState } from "react";
import { getActivitiesLogPagingAsyncThunk } from "../../store/reducers/userRoleSubscriptionSlice";
import { v4 as uuidv4 } from 'uuid';
import dayjsCustom from "../../utils/dayjs/dayjs-custom";
import RoleSubscription from "../../components/role-subscription/RoleSubscription";
import { EActivityType } from "../../models/enums/EActivityType";
import classNames from "classnames";

const UserRoleSubscriptionPage: React.FC = () => {
    const { userId } = useParams();
    const [t] = useTranslation();
    const dispatch = useAppDispatch();

    const { loading, activities, totalRecords, roleSubscription } = useSelector((state: StoreState) => state.userRoleSubscription);

    // Paging
    const [pageIndex, setPageIndex] = useState(1);
    const [pageSize, setPageSize] = useState(5);
    const [activityType, setActivityType] = useState(EActivityType.Subscription);

    useEffect(() => {
        if (userId) {
            dispatch(getActivitiesLogPagingAsyncThunk({
                id: userId,
                params: {
                    pageNumber: pageIndex,
                    pageSize
                },
                activityType
            }));
        }

    }, [dispatch, userId, pageIndex, pageSize, activityType])

    const reloadPaging = () => {
        if (userId) {
            dispatch(getActivitiesLogPagingAsyncThunk({
                id: userId,
                params: {
                    pageNumber: pageIndex,
                    pageSize
                },
                activityType
            }));
        }
    }

    return (
        <>
            <div className="page-wrapper">
                {/* Page Content*/}
                <div className="page-content-tab">
                    <div className="container-fluid">
                        {/* Page-Title */}
                        <div className="row">
                            <div className="col-sm-12">
                                <div className="page-title-box">
                                    <div className="float-end">
                                        <ol className="breadcrumb">
                                            <li className="breadcrumb-item">
                                                <a href="crm-contacts.html#">Dashboard</a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="breadcrumb-item">
                                                <a href="crm-contacts.html#">CMS</a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="breadcrumb-item">
                                                <Link to={"/users"} >Users</Link>
                                            </li>
                                            <li className="breadcrumb-item">
                                                {roleSubscription?.fullName}
                                            </li>
                                        </ol>
                                    </div>
                                    <h4 className="page-title">{t("role_subscription.title")}</h4>
                                </div>
                                {/*end page-title-box*/}
                            </div>
                            {/*end col*/}
                        </div>
                        {/* end page title end breadcrumb */}
                        <RoleSubscription userId={userId} reloadPaging={reloadPaging} />
                        <div className="row">
                            <div className="col-12">
                                <div className="card">
                                    <div className="card-header">
                                        <div className="row align-items-center">
                                            <div className="col">
                                                <h4 className="card-title">{t('role_subscription.activity_header')}</h4>
                                            </div>
                                            {/*end col*/}
                                        </div>{" "}
                                        {/*end row*/}
                                    </div>
                                    {/*end card-header*/}
                                    <div className="card-body">
                                        <div className="row">
                                            <div className="button-items">
                                                <button
                                                    onClick={() => setActivityType(EActivityType.Subscription)}
                                                    className={classNames("btn", {
                                                        "btn-primary": activityType === EActivityType.Subscription,
                                                        "btn-outline-primary": activityType !== EActivityType.Subscription
                                                    })}><i className="fa-brands fa-servicestack"></i> {t('role_subscription.subscription')}</button>

                                                <button
                                                    onClick={() => setActivityType(EActivityType.Payment)}
                                                    className={classNames("btn", {
                                                        "btn-primary": activityType === EActivityType.Payment,
                                                        "btn-outline-primary": activityType !== EActivityType.Payment
                                                    })}><i className="fa-solid fa-wallet"></i> {t('role_subscription.payment')}</button>

                                            </div>
                                        </div>
                                        <div className="table-responsive">
                                            <table className="table table-hover">
                                                {!loading && <caption className="pt-2 pb-0">{t('paging.caption', {
                                                    start: ((pageIndex - 1) * pageSize) + 1,
                                                    end: ((pageIndex - 1) * pageSize) + activities.length,
                                                    total: totalRecords
                                                })}</caption>}
                                                <thead>
                                                    <tr>
                                                        <th>{t('role_subscription.activity_id')}</th>
                                                        <th>{t('role_subscription.acitivy_type')}</th>
                                                        <th>{t('role_subscription.activity_description')}</th>
                                                        <th>{t('role_subscription.activity_created_on')}</th>
                                                        <th>{t('role_subscription.activity_ipv4_address')}</th>
                                                        <th>{t('role_subscription.activity_ipv6_address')}</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    {activities.map((activity) => (
                                                        <tr key={uuidv4()}>
                                                            <td>{activity.id}</td>
                                                            <td>{activity.activityType}</td>
                                                            <td>{activity.description}</td>
                                                            <td>{dayjsCustom.utc(activity.createdOnUtc).local().format('DD-MM-YYYY HH:mm')}</td>
                                                            <td></td>
                                                            <td></td>
                                                        </tr>
                                                    ))}
                                                </tbody>
                                            </table>
                                        </div>
                                        <div className="row mt-2">
                                            <div className="col">
                                                <button className="btn btn-outline-light btn-sm px-4"
                                                >
                                                    + {t('user.add_new')}
                                                </button>
                                            </div>
                                            <div className="col">
                                                <select className="form-select"
                                                    style={{ width: "auto" }}
                                                    value={pageSize}
                                                    onChange={(event: React.ChangeEvent<HTMLSelectElement>) => setPageSize(Number(event.target.value))}>
                                                    <option value={5}>5</option>
                                                    <option value={10}>10</option>
                                                    <option value={15}>15</option>
                                                    <option value={25}>25</option>
                                                    <option value={35}>35</option>
                                                </select>
                                            </div>{" "}
                                            {/*end col*/}
                                            <div className="col-auto">
                                                <nav aria-label="...">
                                                    <Pagination
                                                        pageIndex={pageIndex}
                                                        totalCounts={totalRecords}
                                                        pageSize={pageSize}
                                                        onPageChange={page => setPageIndex(page)} />
                                                    {/*end pagination*/}
                                                </nav>
                                                {/*end nav*/}
                                            </div>{" "}
                                            {/*end col*/}
                                        </div>
                                        {/*end row*/}
                                    </div>
                                    {/*end card-body*/}
                                </div>
                                {/*end card*/}
                            </div>{" "}
                            {/*end col*/}
                        </div>
                        {/*end row*/}
                        {/* container */}
                    </div>
                    {/* end page content */}
                </div>
            </div>
        </>
    );
}

export default UserRoleSubscriptionPage;