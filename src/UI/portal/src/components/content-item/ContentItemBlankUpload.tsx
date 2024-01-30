import { ActualFileObject, FilePondFile } from "filepond";
import React, { useState } from "react";
import { FilePond } from "react-filepond";
import { useTranslation } from "react-i18next";
import { toast } from "react-toastify";
import { createContentItems } from "../../services/content-item/cotentItemService";
import { RegexHelper } from "../../utils/regex";

type ContentItemBlankUploadProps = {
    id: string | undefined;
}

const ContentItemBlankUpload: React.FC<ContentItemBlankUploadProps> = ({ id }: ContentItemBlankUploadProps) => {
    const [t] = useTranslation();

    const [files, setFiles] = useState<(ActualFileObject)[]>([]);

    const onUpdateFiles = (filesPondFiles: FilePondFile[]) => {
        const files = filesPondFiles.sort((a, b) => RegexHelper.getNumberByText(a.filename) - RegexHelper.getNumberByText(b.filename)).map(item => item.file);
        setFiles(files);
    }

    const onSubmit = async () => {
        if (files.length === 0) {
            toast.warning(t("toast.please_select_at_least_one_file_to_upload"), {
                hideProgressBar: true,
                autoClose: 2000
            })
            return;
        }

        const formData = new FormData();
        for (const element of files) {
            formData.append("files", element);
        }

        const toastId = toast.loading(t("toast.please_wait"), {
            hideProgressBar: true
        });

        if (id && Number(id)) {
            await createContentItems(Number(id), formData);

            toast.update(toastId, {
                render: t("toast.create_inprogress_sucessfully"), type: toast.TYPE.SUCCESS, isLoading: false,
                autoClose: 2000
            });

            return;
        }

        toast.done(toastId);
    }

    const onReset = () => {
        setFiles([]);
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
                    <div className="row">
                        <div className="row">
                            <div className="col-12">
                                <div className="card">
                                    <div className="card-header">
                                        <div className="row align-items-center">
                                            <div className="col">
                                                <h4 className="card-title">{t('content_item.title')}</h4>
                                                <p className="text-muted mb-0">{t('content_item.description')}</p>
                                            </div>
                                            {/*end col*/}
                                        </div>{" "}
                                        {/*end row*/}
                                    </div>
                                    {/*end card-header*/}
                                    <div className="card-body">
                                        <div className="d-grid">
                                            <FilePond
                                                files={files}
                                                onupdatefiles={onUpdateFiles}
                                                allowMultiple={true}
                                                maxFiles={250}
                                                name="files"
                                                labelIdle='Drag & Drop your files or <span class="filepond--label-action">Browse</span>'
                                                beforeAddFile={(file) => {
                                                    return new Promise((resolve) => {
                                                        if (!file.fileType.includes('image/')) {
                                                            resolve(false);
                                                        }
                                                        resolve(true);
                                                    })
                                                }}
                                            />
                                        </div>
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
                                    </div>
                                    {/*end card-body*/}
                                </div>
                                {/*end card*/}
                            </div>{" "}
                            {/*end col*/}
                        </div>
                        {/*end row*/}
                    </div>

                    {/*end row*/}
                    {/* container */}
                </div>
                {/* end page content */}
            </div>
        </div>

    );
}

export default React.memo(ContentItemBlankUpload);