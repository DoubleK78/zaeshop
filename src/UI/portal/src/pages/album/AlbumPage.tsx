import React, { useCallback, useEffect, useMemo, useState } from "react";
import { useSelector } from "react-redux";
import { StoreState, useAppDispatch } from '../../store';
import { getAlbumsPagingAsyncThunk, getAlbumAlertMessagesAsyncThunk, getAllContentTypesAsyncThunk } from "../../store/reducers/albumSlice";
import AlbumPagingResponse from "../../models/album/AlbumPagingResponse";
import { ActionTypeGrid } from "../../models/enums/ActionTypeGrid";
import { useTranslation } from "react-i18next";
import ModalCommon from "../../components/shared/ModalCommon";
import Pagination from "../../components/shared/Pagination";
import { v4 as uuidv4 } from 'uuid';
import dayjsCustom from "../../utils/dayjs/dayjs-custom";
import CreateAlbum from "../../components/album/CreateAlbum";
import UpdateAlbum from "../../components/album/UpdateAlbum";
import DeleteAlbum from "../../components/album/DeleteAlbum";
import { Link } from "react-router-dom";
import { useDebounce } from "use-debounce";

const AlbumPage: React.FC = () => {
    const [isOpen, setIsOpen] = useState(false);
    const [actionGrid, setActionGrid] = useState(ActionTypeGrid.CREATE);

    const [t] = useTranslation();

    const albumState = useSelector((state: StoreState) => state.album);
    const dispatch = useAppDispatch();

    const albums = useMemo(() => albumState.albums, [albumState.albums]);
    const [album, setAlbum] = useState<AlbumPagingResponse>(albums[0] || null);

    // Paging
    const [pageIndex, setPageIndex] = useState(1);
    const [pageSize, setPageSize] = useState(15);
    const [search, setSearch] = useState<string>('');
    const [sortColumn, setSortColumn] = useState<string>('title');
    const [sortDirection, setSortDirection] = useState<string>('asc');
    const [viewByTopType, setViewByTopType] = useState<string>('');
    const [region, setRegion] = useState<string>('vi');

    const [debouncedSearchValue] = useDebounce(search, 500);

    useEffect(() => {
        dispatch(getAllContentTypesAsyncThunk());
        dispatch(getAlbumAlertMessagesAsyncThunk())
    }, [dispatch]);

    useEffect(() => {
        dispatch(getAlbumsPagingAsyncThunk({
            pageNumber: pageIndex,
            pageSize,
            searchTerm: debouncedSearchValue?.trim(),
            sortColumn,
            sortDirection,
            topType: viewByTopType,
            region
        }));
    }, [dispatch, pageIndex, pageSize, debouncedSearchValue, sortColumn, sortDirection, viewByTopType, region]);

    const openModal = (actionGrid: ActionTypeGrid, album?: AlbumPagingResponse) => {
        setActionGrid(actionGrid);
        if (album) {
            setAlbum(album);
        }
        setIsOpen(true);
    }

    const closeModal = (isReload?: boolean) => {
        if (isReload) {
            dispatch(getAlbumsPagingAsyncThunk({
                pageNumber: pageIndex,
                pageSize,
                searchTerm: debouncedSearchValue?.trim(),
                sortColumn,
                sortDirection,
                topType: viewByTopType,
                region
            }));
        }
        setIsOpen(false);
    }

    const BodyModal = useCallback((actionGrid: ActionTypeGrid) => {
        switch (actionGrid) {
            case ActionTypeGrid.CREATE:
                return CreateAlbum;
            case ActionTypeGrid.EDIT:
                return UpdateAlbum;
            case ActionTypeGrid.DELETE:
                return DeleteAlbum;
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
                                            <li className="breadcrumb-item active">Albums</li>
                                        </ol>
                                    </div>
                                    <h4 className="page-title">{t("album.title")}</h4>
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
                                                <h4 className="card-title">{t('album.title_detail')}</h4>
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
                                                    placeholder={t('album.search_placeholder')}
                                                    onChange={(e) => setSearch(e.target.value)}
                                                />
                                            </div>
                                            <div className="general-label">
                                                <div className="row row-cols-lg-auto align-items-center">
                                                    <div className="col">
                                                        <label>{t('album.sort_column_label')}</label>
                                                        <select className="form-select"
                                                            style={{ width: "auto" }}
                                                            value={sortColumn}
                                                            onChange={(event: React.ChangeEvent<HTMLSelectElement>) => setSortColumn((event.target.value))}>
                                                            <option value={'title'}>{t('album.sort_column_title')}</option>
                                                            <option value={'createdOnUtc'}>{t('album.sort_column_uploaded_on')}</option>
                                                            <option value={'views'}>{t('album.sort_column_views')}</option>
                                                            <option value={'viewByTopType'}>{t('album.sort_column_view_by_top_type')}</option>
                                                        </select>
                                                    </div>
                                                    <div className="col">
                                                        <label>{t('album.sort_direction_label')}</label>
                                                        <select className="form-select"
                                                            style={{ width: "auto" }}
                                                            value={sortDirection}
                                                            onChange={(event: React.ChangeEvent<HTMLSelectElement>) => setSortDirection((event.target.value))}>
                                                            <option value={'asc'}>{t('album.sort_direction_asc')}</option>
                                                            <option value={'desc'}>{t('album.sort_direction_desc')}</option>
                                                        </select>
                                                    </div>
                                                    <div className="col">
                                                        <label>{t('album.filter_top_type')}</label>
                                                        <select className="form-select"
                                                            style={{ width: "auto" }}
                                                            value={viewByTopType}
                                                            onChange={(event: React.ChangeEvent<HTMLSelectElement>) => setViewByTopType((event.target.value))}>
                                                            <option value={''}>{t('album.filter_top_type_none')}</option>
                                                            <option value={'day'}>{t('album.filter_top_type_day')}</option>
                                                            <option value={'week'}>{t('album.filter_top_type_week')}</option>
                                                            <option value={'month'}>{t('album.filter_top_type_month')}</option>
                                                            <option value={'year'}>{t('album.filter_top_type_year')}</option>
                                                        </select>
                                                    </div>
                                                    <div className="col">
                                                        <label>{t('album.filter_region')}</label>
                                                        <select className="form-select"
                                                            style={{ width: "auto" }}
                                                            value={region}
                                                            onChange={(event: React.ChangeEvent<HTMLSelectElement>) => setRegion((event.target.value))}>
                                                            <option value={''}>{t('album.filter_region_all')}</option>
                                                            <option value={'vi'}>{t('album.filter_region_vietnam')}</option>
                                                            <option value={'en'}>{t('album.filter_region_english')}</option>
                                                        </select>
                                                    </div>
                                                </div>
                                            </div>
                                            <table className="table table-hover">
                                                {!albumState.loading && <caption className="pt-2 pb-0">{t('paging.caption', {
                                                    start: ((pageIndex - 1) * pageSize) + 1,
                                                    end: ((pageIndex - 1) * pageSize) + albumState.albums.length,
                                                    total: albumState.totalRecords
                                                })}</caption>}
                                                <thead>
                                                    <tr>
                                                        <th>{t('album.id')}</th>
                                                        <th>{t('album.album_title')}</th>
                                                        <th>{t('album.album_description')}</th>
                                                        <th>{t('album.album_content_types')}</th>
                                                        <th>{t('album.created_on')}</th>
                                                        <th>{t('album.updated_on')}</th>
                                                        <th>{t('album.views')}</th>
                                                        <th>{t('album.view_by_top_type')}</th>
                                                        <th>{t('album.action')}</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    {albums.map((album) => (
                                                        <tr key={uuidv4()}>
                                                            <td>{album.id}</td>
                                                            <td>
                                                                <Link className="link-primary link-offset-2 link-underline-opacity-25 link-underline-opacity-100-hover"
                                                                    to={`${album.id}`}>{album.title}
                                                                </Link>
                                                            </td>
                                                            <td>{album.description}</td>
                                                            <td>{album.contentTypes}</td>
                                                            <td>{dayjsCustom.utc(album.createdOnUtc).local().format('DD-MM-YYYY HH:mm')}</td>
                                                            <td>{album.updatedOnUtc && dayjsCustom.utc(album.updatedOnUtc).local().format('DD-MM-YYYY HH:mm')}</td>
                                                            <td>{album.views}</td>
                                                            <td>{album.viewByTopType}</td>
                                                            <td>
                                                                <button className="btn"
                                                                    onClick={() => openModal(ActionTypeGrid.EDIT, album)}>
                                                                    <i className="fa-solid fa-pen text-secondary font-16"></i>
                                                                </button>
                                                                <button className="btn"
                                                                    onClick={() => openModal(ActionTypeGrid.DELETE, album)}>
                                                                    <i className="fa-solid fa-trash text-secondary font-16"></i>
                                                                </button>
                                                            </td>
                                                        </tr>
                                                    ))}
                                                </tbody>
                                            </table>
                                        </div>
                                        <div className="row">
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
                                                        totalCounts={albumState.totalRecords}
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
                props={{ modalIsOpen: isOpen, openModal, closeModal, album }}
                Component={BodyModal(actionGrid)}
            />
        </>
    );
};

export default AlbumPage;
