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
	installer, err := t.Dagger.installer(ctx, "sdk")
	if err != nil {
		return err
	}
	introspection, err := t.Dagger.introspection(ctx, installer)
	if err != nil {
		return err
	}
	src := t.Dagger.Src.Directory("sdk/dotnet")
	return dag.DotnetSDKDev(dagger.DotnetSDKDevOpts{Source: src}).Test(ctx, introspection)
}

func (t DotnetSDK) TestPublish(ctx context.Context, tag string) error {
	// The SDK doesn't publish as a library at the moment.
	return nil
}

func (t DotnetSDK) Generate(ctx context.Context) (*dagger.Directory, error) {
	installer, err := t.Dagger.installer(ctx, "sdk")
	if err != nil {
		return nil, err
	}
	introspection, err := t.Dagger.introspection(ctx, installer)
	if err != nil {
		return nil, err
	}
	src := t.Dagger.Src.Directory("sdk/dotnet")

	return dag.
		Directory().
		WithDirectory(
			"sdk/dotnet",
			dag.DotnetSDKDev(dagger.DotnetSDKDevOpts{Source: src}).Generate(introspection),
		), nil
}

func (t DotnetSDK) Bump(ctx context.Context, version string) (*dagger.Directory, error) {
	// The SDK has no engine to bump at the moment. So skip it.
	return dag.Directory(), nil
}
