import { useTranslation } from "react-i18next";
import AlbumDetail from "../../components/album-detail-collection/AlbumDetail";
import { Link, useParams } from "react-router-dom";
import AlbumExtraInfo from "../../components/album-detail-collection/AlbumExtraInfo";
import React, { useCallback, useEffect, useState } from "react";
import Pagination from "../../components/shared/Pagination";
import { ActionTypeGrid } from "../../models/enums/ActionTypeGrid";
import { StoreState, useAppDispatch } from "../../store";
import { useSelector } from "react-redux";
import CollectionPagingResponse from "../../models/album-detail-collection/CollectionPagingResponse";
import { getCollectionPagingAsyncThunk } from "../../store/reducers/albumDetailCollectionSlice";
import { v4 as uuidv4 } from 'uuid';
import dayjsCustom from "../../utils/dayjs/dayjs-custom";
import ModalCommon from "../../components/shared/ModalCommon";
import CreateCollection from "../../components/album-detail-collection/CreateCollection";
import UpdateCollection from "../../components/album-detail-collection/UpdateCollection";
import DeleteCollection from "../../components/album-detail-collection/DeleteCollection";
import { useDebounce } from "use-debounce";

const AlbumDetailCollectionPage: React.FC = () => {
    const [isOpen, setIsOpen] = useState(false);
    const [actionGrid, setActionGrid] = useState(ActionTypeGrid.CREATE);

    const [t] = useTranslation();
    const { albumId } = useParams();

    const { collections, totalRecords, loading } = useSelector((state: StoreState) => state.albumDetailCollection);
    const [collection, setCollection] = useState<CollectionPagingResponse>(collections[0] || null);

    const dispatch = useAppDispatch();

    // Paging
    const [pageIndex, setPageIndex] = useState(1);
    const [pageSize, setPageSize] = useState(5);
    const [search, setSearch] = useState<string>('');
    const [debouncedSearchValue] = useDebounce(search, 500);
    const [sortColumn, setSortColumn] = useState<string>('title');
    const [sortDirection, setSortDirection] = useState<string>('desc');

    useEffect(() => {
        if (albumId && Number(albumId)) {
            dispatch(getCollectionPagingAsyncThunk({
                pageNumber: pageIndex,
                pageSize,
                albumId: Number(albumId),
                searchTerm: debouncedSearchValue?.trim(),
                sortColumn,
                sortDirection
            }));
        }
    }, [dispatch, albumId, pageIndex, pageSize, debouncedSearchValue, sortColumn, sortDirection]);

    const openModal = (actionGrid: ActionTypeGrid, collection?: CollectionPagingResponse) => {
        setActionGrid(actionGrid);
        if (collection) {
            setCollection(collection);
        }
        setIsOpen(true);
    }

    const closeModal = (isReload?: boolean) => {
        if (isReload) {
            dispatch(getCollectionPagingAsyncThunk({ pageNumber: pageIndex, pageSize, albumId: Number(albumId) }));
        }
        setIsOpen(false);
    }

    const BodyModal = useCallback((actionGrid: ActionTypeGrid) => {
        switch (actionGrid) {
            case ActionTypeGrid.CREATE:
                return CreateCollection;
            case ActionTypeGrid.EDIT:
                return UpdateCollection;
            case ActionTypeGrid.DELETE:
                return DeleteCollection;
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
                                            <li className="breadcrumb-item active">Album Detail</li>
                                        </ol>
                                    </div>
                                    <h4 className="page-title">{t("album.title")}</h4>
                                </div>
                                {/*end page-title-box*/}
                            </div>
                            {/*end col*/}
                        </div>
                        {/* end page title end breadcrumb */}
                        <AlbumDetail id={albumId} />
                        <AlbumExtraInfo id={albumId} />
                        <div className="row">
                            <div className="col-12">
                                <div className="card">
                                    <div className="card-header">
                                        <div className="row align-items-center">
                                            <div className="col">
                                                <h4 className="card-title">{t('album_detail.title_collection')}</h4>
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
                                                            <option value={'title'}>{t('album_detail.sort_column_title')}</option>
                                                            <option value={'createdOnUtc'}>{t('album_detail.sort_column_uploaded_on')}</option>
                                                            <option value={'updatedOnUtc'}>{t('album_detail.sort_column_updated_on')}</option>
                                                            <option value={'views'}>{t('album_detail.sort_column_views')}</option>
                                                            <option value={'levelPublic'}>{t('album_detail.level_public')}</option>
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
                                                {!loading && <caption className="pt-2 pb-0">{t('paging.caption', {
                                                    start: ((pageIndex - 1) * pageSize) + 1,
                                                    end: ((pageIndex - 1) * pageSize) + collections.length,
                                                    total: totalRecords
                                                })}</caption>}
                                                <thead>
                                                    <tr>
                                                        <th>{t('album_detail.id')}</th>
                                                        <th>{t('album_detail.title')}</th>
                                                        <th>{t('album_detail.level_public')}</th>
                                                        <th>{t('album_detail.volume')}</th>                                     
                                                        <th>{t('album_detail.description')}</th>
                                                        <th>{t('album_detail.created_on')}</th>
                                                        <th>{t('album_detail.updated_on')}</th>
                                                        <th>{t('album_detail.views')}</th>
                                                        <th>{t('album_detail.action')}</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    {collections.map((collection) => (
                                                        <tr key={uuidv4()}>
                                                            <td>{collection.id}</td>
                                                            <td>
                                                                <Link className="link-primary link-offset-2 link-underline-opacity-25 link-underline-opacity-100-hover"
                                                                    to={`/collections/${collection.id}`}>{collection.title}
                                                                </Link>
                                                            </td>
                                                            <td>{collection.levelPublic}</td>
                                                            <td>{collection.volume}</td>
                                                            <td>{collection.description}</td>
                                                            <td>{dayjsCustom.utc(collection.createdOnUtc).local().format('DD-MM-YYYY HH:mm')}</td>
                                                            <td>{collection.updatedOnUtc && dayjsCustom.utc(collection.updatedOnUtc).local().format('DD-MM-YYYY HH:mm')}</td>
                                                            <td>{collection.views}</td>
                                                            <td>
                                                                <button className="btn"
                                                                    onClick={() => openModal(ActionTypeGrid.EDIT, collection)}>
                                                                    <i className="fa-solid fa-pen text-secondary font-16"></i>
                                                                </button>
                                                                <button className="btn"
                                                                    onClick={() => openModal(ActionTypeGrid.DELETE, collection)}>
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
                                                <Link className="btn btn-primary btn-sm px-4"
                                                    to={`/albums/${albumId}/bulk-create`}>
                                                    + {t('album_detail.bulk_create')}
                                                </Link>
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
            <ModalCommon
                props={{ modalIsOpen: isOpen, openModal, closeModal, collection, albumId }}
                Component={BodyModal(actionGrid)}
            />
        </>
    );
}

export default AlbumDetailCollectionPage;