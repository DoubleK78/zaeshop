import React, { useCallback, useEffect, useMemo, useState } from "react";
import { useSelector } from "react-redux";
import { StoreState, useAppDispatch } from '../store';
import { getAlbumsPagingAsyncThunk } from "../store/reducers/albumSlice";
import AlbumPagingResponse from "../models/album/AlbumPagingResponse";
import { ActionTypeGrid } from "../models/enums/ActionTypeGrid";
import { useTranslation } from "react-i18next";
import ModalCommon from "../components/shared/ModalCommon";
import Pagination from "../components/shared/Pagination";
import { v4 as uuidv4 } from 'uuid';
import dayjs from "dayjs";

const AlbumPage: React.FC = () => {
    const [isOpen, setIsOpen] = useState(false);
    const [actionGrid, setActionGrid] = useState(ActionTypeGrid.CREATE);

    const [t] = useTranslation();

    const albumState = useSelector((state: StoreState) => state.album);
    const dispatch = useAppDispatch();

    const albums = useMemo(() => albumState.albums, [albumState.albums]);
    const [album, setAlbum] = useState<AlbumPagingResponse>(albums[0] || null);

    console.log(albums);

    // Paging
    const [pageIndex, setPageIndex] = useState(1);
    const [pageSize, setPageSize] = useState(5);

    useEffect(() => {
        dispatch(getAlbumsPagingAsyncThunk({ pageIndex, pageSize }));
    }, [dispatch, pageIndex, pageSize]);

    const openModal = (actionGrid: ActionTypeGrid, album?: AlbumPagingResponse) => {
        setActionGrid(actionGrid);
        if (album) {
            setAlbum(album);
        }
        setIsOpen(true);
    }

    const closeModal = () => {
        setIsOpen(false);
    }

    const BodyModal = useCallback((actionGrid: ActionTypeGrid) => {
        // switch (actionGrid) {
        //     case ActionTypeGrid.CREATE:
        //         return CreateUser;
        //     case ActionTypeGrid.EDIT:
        //         return UpdateUser;
        //     case ActionTypeGrid.DELETE:
        //         return DeleteUser;
        // }
        return React.Fragment;
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
                                            <table className="table">
                                                <thead>
                                                    <tr>
                                                        <th>{t('album.id')}</th>
                                                        <th>{t('album.album_title')}</th>
                                                        <th>{t('album.album_description')}</th>
                                                        <th>{t('album.album_content_types')}</th>
                                                        <th>{t('album.created_on')}</th>
                                                        <th>{t('album.updated_on')}</th>
                                                        <th>{t('album.action')}</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    {albums.map((album) => (
                                                        <tr key={uuidv4()}>
                                                            <td>{album.id}</td>
                                                            <td>{album.title}</td>
                                                            <td>{album.description}</td>
                                                            <td>{album.contentTypes}</td>
                                                            <td>{dayjs(album.createdOnUtc).format('DD-MM-YYYY HH:mm')}</td>
                                                            <td>{album.updatedOnUtc && dayjs(album.updatedOnUtc).format('DD-MM-YYYY HH:mm')}</td>
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
