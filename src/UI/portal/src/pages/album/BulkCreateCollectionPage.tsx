import { Link, useParams } from "react-router-dom";
import { StoreState, useAppDispatch } from "../../store";
import { useTranslation } from "react-i18next";
import { useSelector } from "react-redux";
import { useCallback, useEffect, useMemo, useState } from "react";
import { getAlbumDetailAsyncThunk } from "../../store/reducers/albumDetailCollectionSlice";
import { useDropzone } from 'react-dropzone';
import { BulkCreateCollectionRequest } from "../../models/album-detail-collection/BulkCreateCollectionRequest";
import { RegexHelper } from "../../utils/regex";
import { toast } from "react-toastify";
import { bulkCreateCollectionItemApi } from "../../services/album-detail-collection/bulkCreateCollectionService";

const baseStyle: any = {
    flex: 1,
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    padding: '20px',
    borderWidth: 2,
    borderRadius: 2,
    borderColor: '#eeeeee',
    borderStyle: 'dashed',
    backgroundColor: '#fafafa',
    color: '#bdbdbd',
    outline: 'none',
    transition: 'border .24s ease-in-out'
};

const BulkCreateCollectionPage = () => {
    const { albumId } = useParams();
    const dispatch = useAppDispatch();
    const [t] = useTranslation();
    const { albumDetail } = useSelector((state: StoreState) => state.albumDetailCollection);
    const [files, setFiles] = useState<File[]>([]);
    const [albumFolderName, setAlbumFolderName] = useState<string | null>(null);

    const onDrop = useCallback((acceptedFiles: File[]) => {
        setFiles([...files, ...acceptedFiles])
    }, [files]);
    const { getRootProps, getInputProps } = useDropzone({ accept: { 'image/*': [] }, onDrop });

    useEffect(() => {
        dispatch(getAlbumDetailAsyncThunk({ id: albumId }));
    }, [dispatch, albumId]);

    const onReset = () => {
        setFiles([]);
        setAlbumFolderName(null);
    }

    const previewCollectionGroupBy = useMemo(() => {
        if (files.length > 0) {
            if (files.length == 0) {
                return [];
            }
            const firstFile: any = files[0];
            const isAlbumFolder = firstFile.path.split("/").length > 3;
            const isCollectionFolder = firstFile.path.split("/").length > 2;
            // Case 1: Albums/Collections/Items
            if (isAlbumFolder) {
                const folderGroupByReduce = files.reduce((folderGroups: BulkCreateCollectionRequest[], file: any) => {
                    const folderName = file.path.split('/')[2];
                    const folder = folderGroups.find(folderGroup => folderGroup.name === folderName);

                    if (folder) {
                        folder.contentItems.push({
                            name: file.name,
                            size: file.size
                        });
                    }
                    else {
                        folderGroups.push({
                            name: folderName,
                            contentItems: [{
                                name: file.name,
                                size: file.size
                            }]
                        });
                    }

                    return folderGroups;
                }, []);
                setAlbumFolderName(firstFile.path.split("/")[1]);

                return folderGroupByReduce;
            }
            // Case 2: Collections/Items
            else if (isCollectionFolder) {
                const folderGroupByReduce = files.reduce((folderGroups: BulkCreateCollectionRequest[], file: any) => {
                    const folderName = file.path.split('/')[1];
                    const folder = folderGroups.find(folderGroup => folderGroup.name === folderName);

                    if (folder) {
                        folder.contentItems.push({
                            name: file.name,
                            size: file.size
                        });
                    }
                    else {
                        folderGroups.push({
                            name: folderName,
                            contentItems: [{
                                name: file.name,
                                size: file.size
                            }]
                        });
                    }

                    return folderGroups;
                }, []);

                setAlbumFolderName(null);

                return folderGroupByReduce;
            }

            setAlbumFolderName(null);
        }
        return [];
    }, [files]);

    const onDeleteFolder = (folderName: string) => {
        setFiles(files.filter((file: any) => file.path.split("/")[1] !== folderName));
    }

    const listFiles = useMemo(() => {
        return previewCollectionGroupBy.sort((a, b) => RegexHelper.getNumberByText(a.name) - RegexHelper.getNumberByText(b.name)).map((item, index) => (
            <li key={index}>
                {item.name}
                <button className="btn"
                    onClick={() => onDeleteFolder(item.name)}>
                    <i className="fa-solid fa-trash text-danger font-16"></i>
                </button>
                <ul>
                    {item.contentItems.sort((a, b) => RegexHelper.getNumberByText(a.name) - RegexHelper.getNumberByText(b.name)).map((contentItem, index) => (
                        <li key={index}>
                            {contentItem.name} - {contentItem.size} bytes
                        </li>
                    ))}
                </ul>
            </li>
        ));
    }, [previewCollectionGroupBy]);

    const onSubmit = async () => {
        const toastId = toast.loading(t("toast.please_wait"), {
            hideProgressBar: true
        });

        const response = await bulkCreateCollectionItemApi(albumId!, previewCollectionGroupBy);
        if (response.status === 200) {
            toast.update(toastId, {
                render: t("toast.create_sucessfully"), type: toast.TYPE.SUCCESS, isLoading: false,
                autoClose: 2000
            });
        }

        toast.done(toastId);
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
                                            <li className="breadcrumb-item active">Album Detail</li>
                                            <li className="breadcrumb-item active">
                                                <Link to={`/albums/${albumId}`}>{albumDetail?.title}</Link>
                                            </li>
                                            <li className="breadcrumb-item active">{t("album_detail.bulk_create")}</li>
                                        </ol>
                                    </div>
                                    <h4 className="page-title">{t("album_detail.bulk_create")}</h4>
                                </div>
                                {/*end page-title-box*/}
                            </div>
                            {/*end col*/}
                        </div>
                        {/* end page title end breadcrumb */}
                        <div className="row mb-3">
                            <div className="row">
                                <div className="row">
                                    <div className="col-12">
                                        <div className="card">
                                            <div className="card-header">
                                                <div className="row align-items-center">
                                                    <div className="col">
                                                        <h4 className="card-title">{t('album_detail.bulk_create_sub_title')}</h4>
                                                        <p className="text-muted mb-0">{t('album_detail.bulk_create_sub_description')}</p>
                                                    </div>
                                                    {/*end col*/}
                                                </div>{" "}
                                                {/*end row*/}
                                            </div>
                                            {/*end card-header*/}
                                            <div className="card-body">
                                                <button
                                                    type="button"
                                                    className="btn btn-light"
                                                    onClick={onReset}
                                                >
                                                    {t('content_item.reset')}
                                                </button>
                                                <button
                                                    type="button"
                                                    className="btn btn-primary"
                                                    onClick={onSubmit}
                                                >
                                                    {t('content_item.upload_button')}
                                                </button>
                                                <div className="d-grid mt-2">
                                                    <div {...getRootProps({ className: 'dropzone', style: baseStyle })}>
                                                        <input {...getInputProps()} />
                                                        <p>Drag 'n' drop some files here, or click to select files</p>
                                                    </div>
                                                    <aside>
                                                        <h4>{t('album_detail.bulk_create_Preview')}</h4>
                                                        {albumFolderName && <h4 className="text-primary">{albumFolderName}</h4>}
                                                        <ul>{listFiles}</ul>
                                                    </aside>
                                                </div>
                                            </div>
                                            {/*end card-body*/}
                                        </div>
                                        {/*end card*/}
                                    </div>{" "}
                                    {/*end col*/}
                                </div>
                                {/*end row*/}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
}

export default BulkCreateCollectionPage;