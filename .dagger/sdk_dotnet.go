package main

import (
	"context"
	"go.opentelemetry.io/otel/codes"
	"golang.org/x/sync/errgroup"

	"github.com/dagger/dagger/.dagger/internal/dagger"
)

type DotnetSDK struct {
	// +private
	Dagger *DaggerDev
}

func (t DotnetSDK) Lint(ctx context.Context) error {
	// Run all checks in parallel using errgroup
	eg := errgroup.Group{}

	eg.Go(func() (rerr error) {
		ctx, span := Tracer().Start(ctx, "lint the dotnet Go modules")
		defer func() {
			if rerr != nil {
				span.SetStatus(codes.Error, rerr.Error())
			}
			span.End()
		}()
		return dag.
			Go(t.Dagger.WithModCodegen().Source()).
			Lint(ctx, dagger.GoLintOpts{
				Packages: []string{
					"sdk/dotnet/module",
					"sdk/dotnet/bootstrap",
				},
			})
	})

	// TODO: call dotnet native lint command https://github.com/M-Pixel/dagger/issues/22

	return eg.Wait()
}

func (t DotnetSDK) Test(ctx context.Context) error {
	// TODO: Implement https://github.com/M-Pixel/dagger/issues/16
	return nil
}

func (t DotnetSDK) TestPublish(ctx context.Context, tag string) error {
	return dag.DotnetSDKDev().TestPublish(ctx, tag)
}

func (t DotnetSDK) Publish(
	ctx context.Context,
	tag string,
	nugetApiKey *dagger.Secret,
	containerRegistryUrl string,
	containerRegistryUsername string,
	containerRegistryPassword *dagger.Secret,
	dryRun bool,
) error {
	if dryRun {
		return t.TestPublish(ctx, tag)
	}
	return dag.DotnetSDKDev().
		PublishFromCd(ctx, tag, nugetApiKey, containerRegistryUrl, containerRegistryUsername, containerRegistryPassword)
}

func (t DotnetSDK) Generate(ctx context.Context) (*dagger.Directory, error) {
	// Dotnet module generates IL directly, does not need to store derived artifacts in Git
	return dag.Directory(), nil
}

func (t DotnetSDK) Bump(ctx context.Context, version string) (*dagger.Directory, error) {
	return dag.DotnetSDKDev().Bump(version), nil
}
