import { useTranslation } from "react-i18next";
import { StoreState, useAppDispatch } from "../../store";
import { useSelector } from "react-redux";
import { useEffect, useState } from "react";
import { getPaymentsAsyncThunk } from "../../store/reducers/paymentSlice";
import Pagination from "../../components/shared/Pagination";
import { v4 as uuidv4 } from 'uuid';
import classNames from "classnames";
import dayjsCustom from "../../utils/dayjs/dayjs-custom";

const PaymentPage: React.FC = () => {
    const [t] = useTranslation();

    const { payments, totalRecords, loading } = useSelector((state: StoreState) => state.paymentManagement);
    const dispatch = useAppDispatch();

    // Paging
    const [pageIndex, setPageIndex] = useState(1);
    const [pageSize, setPageSize] = useState(15);

    useEffect(() => {
        dispatch(getPaymentsAsyncThunk({
            params: {
                pageNumber: pageIndex,
                pageSize,
                sortColumn: 'createdOnUtc',
                sortDirection: 'desc'
            }
        }));
    }, [dispatch, pageIndex, pageSize]);

    return (
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
                                        {/*end nav-item*/}
                                        <li className="breadcrumb-item">
                                            <a href="crm-contacts.html#">CMS</a>
                                        </li>
                                        {/*end nav-item*/}
                                        <li className="breadcrumb-item active">Paymeent Management</li>
                                    </ol>
                                </div>
                            </div>
                            {/*end page-title-box*/}
                        </div>
                        {/*end col*/}
                    </div>
                    {/* end page title end breadcrumb */}
                    <div className="row">
                        <div className="col-12">
                            <div className="card">
                                <div className="card-header">
                                    <div className="row align-items-center">
                                        <div className="col">
                                            <h4 className="card-title">{t('payment_management.title')}</h4>
                                        </div>
                                        {/*end col*/}
                                    </div>{" "}
                                    {/*end row*/}
                                </div>
                                {/*end card-header*/}
                                <div className="card-body">
                                    <div className="table-responsive">
                                        <table className="table table-hover">
                                            {!loading && <caption className="pt-2 pb-0">{t('paging.caption', {
                                                start: ((pageIndex - 1) * pageSize) + 1,
                                                end: ((pageIndex - 1) * pageSize) + (payments?.length ?? 0),
                                                total: totalRecords
                                            })}</caption>}
                                            <thead>
                                                <tr>
                                                    <th>{t('payment_management.email')}</th>
                                                    <th>{t('payment_management.description')}</th>
                                                    <th>{t('payment_management.created_on')}</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                {payments?.map((payment) => (
                                                    <tr key={uuidv4()}>
                                                        <td className={classNames({ 'text-primary': payment.description?.includes('Subscription') })}>{payment.email}</td>
                                                        <td className={classNames({ 'text-primary': payment.description?.includes('Subscription') })}>{payment.description}</td>
                                                        <td>{dayjsCustom.utc(payment.createdOnUtc).local().format('DD-MM-YYYY HH:mm')}</td>
                                                    </tr>
                                                ))}
                                            </tbody>
                                        </table>
                                    </div>
                                    <div className="row mt-2">
                                        <div className="col">
                                            <button className="btn btn-outline-light btn-sm px-4"> Comments
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
                </div>
                {/* container */}
            </div>
            {/* end page content */}
        </div >
    );
}

export default PaymentPage;