import React, { useCallback, useEffect, useMemo, useState } from 'react';
import dayjs from 'dayjs';
import { StoreState } from '../../store';
import { useDispatch, useSelector } from 'react-redux';
import { getUsers } from '../../store/thunks/userThunk';
import ModalCommon from '../../components/shared/ModalCommon';
import CreateUser from '../../components/user/CreateUser';
import { ActionTypeGrid } from '../../models/enums/ActionTypeGrid';
import UpdateUser from '../../components/user/UpdateUser';
import User from '../../models/user/User';
import { useTranslation } from 'react-i18next';
import Pagination from '../../components/shared/Pagination';
import DeleteUser from '../../components/user/DeleteUser';
import { v4 as uuidv4 } from 'uuid';
import { useDebounce } from 'use-debounce';
import { Link } from 'react-router-dom';

const UserPage: React.FC = () => {
    const [isOpen, setIsOpen] = useState(false);
    const [actionGrid, setActionGrid] = useState(ActionTypeGrid.CREATE);

    const [t] = useTranslation();

    const userState = useSelector((state: StoreState) => state.user);
    const dispatch = useDispatch();

    const users = useMemo(() => userState.users, [userState.users]);
    const [user, setUser] = useState<User>(users[0] || null);

    // Paging
    const [pageIndex, setPageIndex] = useState(1);
    const [pageSize, setPageSize] = useState(5);
    const [search, setSearch] = useState<string>('');
    const [sortColumn, setSortColumn] = useState<string>('createdOnUtc');
    const [sortDirection, setSortDirection] = useState<string>('desc');
    const [debouncedSearchValue] = useDebounce(search, 500);

    useEffect(() => {
        getUsers({
            pageNumber: pageIndex,
            pageSize,
            searchTerm: debouncedSearchValue?.trim(),
            sortColumn,
            sortDirection
        })(dispatch);
    }, [dispatch, pageIndex, pageSize, debouncedSearchValue, sortColumn, sortDirection]);

    const openModal = (actionGrid: ActionTypeGrid, user?: User) => {
        setActionGrid(actionGrid);
        if (user) {
            setUser(user);
        }
        setIsOpen(true);
    }

    const closeModal = (isReload?: boolean) => {
        if (isReload) {
            getUsers({
                pageNumber: pageIndex,
                pageSize,
                searchTerm: debouncedSearchValue?.trim(),
                sortColumn,
                sortDirection
            })(dispatch);
        }
        setIsOpen(false);
    }

    const BodyModal = useCallback((actionGrid: ActionTypeGrid) => {
        switch (actionGrid) {
            case ActionTypeGrid.CREATE:
                return CreateUser;
            case ActionTypeGrid.EDIT:
                return UpdateUser;
            case ActionTypeGrid.DELETE:
                return DeleteUser;
        }
    }, []);

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
                                            <li className="breadcrumb-item active">Users</li>
                                        </ol>
                                    </div>
                                    <h4 className="page-title">{t("user.title")}</h4>
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
                                                <h4 className="card-title">{t('user.title_detail')}</h4>
                                            </div>
                                            {/*end col*/}
                                        </div>{" "}
                                        {/*end row*/}
                                    </div>
                                    {/*end card-header*/}
                                    <div className="card-body">
                                        <div className="table-responsive">
                                            <div className="mb-2">
                                                <input
                                                    type="search"
                                                    name="search"
                                                    className="form-control top-search mb-2"
                                                    placeholder={t('album_detail.search_placeholder')}
                                                    onChange={(e) => setSearch(e.target.value)}
                                                />
                                            </div>
                                            <div className="general-label mb-2">
                                                <div className="row row-cols-lg-auto align-items-center">
                                                    <div className="col">
                                                        <label>{t('album.sort_column_label')}</label>
                                                        <select className="form-select"
                                                            style={{ width: "auto" }}
                                                            value={sortColumn}
                                                            onChange={(event: React.ChangeEvent<HTMLSelectElement>) => setSortColumn((event.target.value))}>
                                                            <option value={'fullName'}>{t('user.sort_column_full_name')}</option>
                                                            <option value={'userName'}>{t('user.sort_column_username')}</option>
                                                            <option value={'email'}>{t('user.sort_column_email')}</option>
                                                            <option value={'createdOnUtc'}>{t('user.sort_column_created_on')}</option>
                                                            <option value={'updatedOnUtc'}>{t('user.sort_column_updated_on')}</option>
                                                            <option value={'isBanned'}>{t('user.sort_column_banned')}</option>
                                                        </select>
                                                    </div>
                                                    <div className="col">
                                                        <label>{t('album_detail.sort_direction_label')}</label>
                                                        <select className="form-select"
                                                            style={{ width: "auto" }}
                                                            value={sortDirection}
                                                            onChange={(event: React.ChangeEvent<HTMLSelectElement>) => setSortDirection((event.target.value))}>
                                                            <option value={'asc'}>{t('album_detail.sort_direction_asc')}</option>
                                                            <option value={'desc'}>{t('album_detail.sort_direction_desc')}</option>
                                                        </select>
                                                    </div>
                                                </div>
                                            </div>
                                            <table className="table table-hover">
                                                {!userState.loading && <caption className="pt-2 pb-0">{t('paging.caption', {
                                                    start: ((pageIndex - 1) * pageSize) + 1,
                                                    end: ((pageIndex - 1) * pageSize) + userState.users.length,
                                                    total: userState.totalRecords
                                                })}</caption>}
                                                <thead>
                                                    <tr>
                                                        <th>{t('user.id')}</th>
                                                        <th>{t('user.full_name')}</th>
                                                        <th>{t('user.username')}</th>
                                                        <th>{t('user.email')}</th>
                                                        <th>{t('user.sort_column_banned')}</th>
                                                        <th>{t('user.created_on')}</th>
                                                        <th>{t('user.roles')}</th>
                                                        <th>{t('user.action')}</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    {users.map((user) => (
                                                        <tr key={uuidv4()}>
                                                            <td>
                                                                <Link className="link-primary link-offset-2 link-underline-opacity-25 link-underline-opacity-100-hover" to={`/users/${user.id}`}>{user.id}</Link>                                                       
                                                                {dayjs().diff(dayjs(user.createdOnUtc), 'day') >= 0 && dayjs().diff(dayjs(user.createdOnUtc), 'day') < 7
                                                                    && <span className="badge bg-soft-success">New</span>}
                                                            </td>
                                                            <td>{user.fullName}</td>
                                                            <td>{user.userName}</td>
                                                            <td>{user.email}</td>
                                                            <td>{user.isBanned ? "Yes" : "No"}</td>
                                                            <td>{dayjs(user.createdOnUtc).format('DD-MM-YYYY HH:mm')}</td>
                                                            <td>
                                                                {user?.roles?.split(',').map(role => (
                                                                    <span key={uuidv4()} className="badge bg-soft-primary">{role}</span>
                                                                ))}
                                                            </td>
                                                            <td>
                                                                <button className="btn"
                                                                    onClick={() => openModal(ActionTypeGrid.EDIT, user)}>
                                                                    <i className="fa-solid fa-pen text-secondary font-16"></i>
                                                                </button>
                                                                <button className="btn"
                                                                    onClick={() => openModal(ActionTypeGrid.DELETE, user)}>
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
                                                <button className="btn btn-outline-light btn-sm px-4"
                                                    onClick={() => openModal(ActionTypeGrid.CREATE)}>
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
                                                        totalCounts={userState.totalRecords}
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
            </div>
            <ModalCommon
                props={{ modalIsOpen: isOpen, openModal, closeModal, user }}
                Component={BodyModal(actionGrid)}
            />
        </>
    );
};

export default UserPage;
