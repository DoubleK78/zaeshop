import { useTranslation } from "react-i18next";
import Pagination from "../../components/shared/Pagination";
import { useEffect, useState } from "react";
import { useSelector } from "react-redux";
import { StoreState, useAppDispatch } from "../../store";
import { getCommentsAsyncThunk } from "../../store/reducers/commentsSlice";
import dayjsCustom from "../../utils/dayjs/dayjs-custom";
import { v4 as uuidv4 } from 'uuid';
import CommentManagementResponse from "../../models/comments/CommentManagementResponse";
import { deleteComment } from "../../services/comments/commentManagementService";

const CommentsPage: React.FC = () => {
    const [t] = useTranslation();

    const { comments, totalRecords, loading } = useSelector((state: StoreState) => state.commentManagement);
    const dispatch = useAppDispatch();

    // Paging
    const [pageIndex, setPageIndex] = useState(1);
    const [pageSize, setPageSize] = useState(15);
    const [isReply, setIsReply] = useState(false);
    const [isDeleted, setIsDeleted] = useState(false);

    // Default Range Almost All Today
    const [startDate, setStartDate] = useState<Date>(dayjsCustom().utc().add(7, 'hours').startOf('day').toDate());
    const [endDate, setEndDate] = useState<Date>(dayjsCustom().utc().add(7, 'hours').startOf('day').add(1, 'day').toDate());

    useEffect(() => {
        dispatch(getCommentsAsyncThunk({
            params: {
                pageNumber: pageIndex,
                pageSize,
                sortColumn: 'createdOnUtc',
                sortDirection: 'desc',
                isDeleted,
                isReply,
                startDate,
                endDate
            }
        }));
    }, [dispatch, pageIndex, pageSize, isReply, isDeleted, startDate, endDate]);

    const onDeleteComment = async (comment: CommentManagementResponse) => {
        const confirm = window.confirm(t("comment_management.confirm_delete", {
            email: comment.email,
            album_friendly_name: comment.albumFriendlyName,
            text: comment.text?.slice(0, 60) + '...'
        }));

        if (confirm) {
            await deleteComment(comment.id);
        }
    }

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
                                        <li className="breadcrumb-item active">Comment Management</li>
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
                                            <h4 className="card-title">{t('comment_management.title')}</h4>
                                        </div>
                                        {/*end col*/}
                                    </div>{" "}
                                    {/*end row*/}
                                </div>
                                {/*end card-header*/}
                                <div className="card-body">
                                    <div className="table-responsive">
                                        <div className="general-label mb-2">
                                            <div className="row row-cols-lg-auto align-items-center">
                                                <div className="col">
                                                    <label>{t('comment_management.date_range')}</label>
                                                    <div className="input-group">
                                                        <input type="date" className="form-control"
                                                            value={dayjsCustom(startDate).format("YYYY-MM-DD")}
                                                            onChange={(event) => setStartDate(
                                                                dayjsCustom(event.target.value, "YYYY-MM-DD").toDate())} />
                                                        <span className="input-group-text">to</span>
                                                        <input type="date" className="form-control"
                                                            value={dayjsCustom(endDate).format("YYYY-MM-DD")}
                                                            onChange={(event) => setEndDate(
                                                                dayjsCustom(event.target.value, "YYYY-MM-DD").toDate())} />
                                                    </div>
                                                </div>
                                                <div className="col mt-4">
                                                    <div className="form-check form-check-inline">
                                                        <label>{t('comment_management.reply')}</label>
                                                        <input
                                                            className="form-check-input"
                                                            type="checkbox"
                                                            id="checkboxIsReply"
                                                            checked={isReply}
                                                            onChange={() => setIsReply(!isReply)}
                                                        />
                                                    </div>
                                                    <div className="form-check form-check-inline">
                                                        <label>{t('comment_management.deleted')}</label>
                                                        <input
                                                            className="form-check-input"
                                                            type="checkbox"
                                                            id="checkboxIsDeleted"
                                                            checked={isDeleted}
                                                            onChange={() => setIsDeleted(!isDeleted)}
                                                        />
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        <table className="table table-hover">
                                            {!loading && <caption className="pt-2 pb-0">{t('paging.caption', {
                                                start: ((pageIndex - 1) * pageSize) + 1,
                                                end: ((pageIndex - 1) * pageSize) + (comments?.length ?? 0),
                                                total: totalRecords
                                            })}</caption>}
                                            <thead>
                                                <tr>
                                                    <th>{t('comment_management.email')}</th>
                                                    <th>{t('comment_management.album_friendly_name')}</th>
                                                    <th>{t('comment_management.text')}</th>
                                                    <th>{t('comment_management.reply')}</th>
                                                    <th>{t('comment_management.created_on')}</th>
                                                    <th>{t('comment_management.action')}</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                {comments?.map((comment) => (
                                                    <tr key={uuidv4()}>
                                                        <td>{comment.email}</td>
                                                        <td>{comment.albumFriendlyName}</td>
                                                        <td><div dangerouslySetInnerHTML={{ __html: comment.text ?? '' }} /></td>
                                                        <td>{comment.isReply ? "Yes" : "No"}</td>
                                                        <td>{dayjsCustom.utc(comment.createdOnUtc).local().format('DD-MM-YYYY HH:mm')}</td>
                                                        <td>
                                                            <button className="btn"
                                                                onClick={() => onDeleteComment(comment)}>
                                                            <i className="fa-solid fa-trash text-secondary font-16"></i>
                                                        </button>
                                                    </td>
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
            {/* end page content */ }
        </div >
    )
}

export default CommentsPage;